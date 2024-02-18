using System.Text;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using PassKeys.Business;
using PassKeys.WebApp.Authentication;
using PassKeys.WebApp.Authentication.Models;
using PassKeys.WebApp.Extensions;
using PassKeys.WebApp.Utils;

namespace PassKeys.WebApp.Controllers;

[ApiController]
[Route("/api/fido2")]
public class Fido2Controller(IFido2 fido2, IUserRepository userRepository, IDistributedCache cache, TokenService tokenService) : ControllerBase
{
    [HttpPost("credential-options")]
    [AllowAnonymous]
    public async Task<ActionResult<CredentialOptionsResponse>> CreateCredentialOptions([FromBody]string userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            return BadRequest("Username is required to create an authentication options");
        }

        var existingUser = await userRepository.GetUserAsync(userName);
        if (existingUser != null)
        {
            return BadRequest($"Username {userName} is already taken!");
        }

        var user = await userRepository.CreateUserAsync(userName);
        var excludedCredentials = user.Credentials;

        var credentialOptions = fido2.RequestNewCredential(user.ToFido2User(),
            excludedCredentials.Select(credential => new PublicKeyCredentialDescriptor(credential.Id)).ToList());
        await cache.SetAsync(user.Id.ToString(), Encoding.UTF8.GetBytes(credentialOptions.ToJson()));
        return Ok(new CredentialOptionsResponse
        {
            Options = credentialOptions,
            UserId = user.Id
        });
    }

    [HttpPut("credential-options")]
    public async Task<ActionResult<CredentialOptionsResponse>> UpdateCredentialOptions()
    {
        var user = await userRepository.GetUserAsync(User.Claims.First(claim => claim.Type == ClaimConstants.UserName).Value);
        var excludedCredentials = user.Credentials;
        var credentialOptions = fido2.RequestNewCredential(user.ToFido2User(),
            excludedCredentials.Select(credential => new PublicKeyCredentialDescriptor(credential.Id)).ToList());
        await cache.SetAsync(user.Id.ToString(), Encoding.UTF8.GetBytes(credentialOptions.ToJson()));
        return Ok(new CredentialOptionsResponse
        {
            Options = credentialOptions,
            UserId = user.Id
        });
    }

    [HttpPost("credential")]
    [AllowAnonymous]
    public async Task<ActionResult<CreateCredentialResponse>> CreateCredential(
        [FromBody] CreateCredentialRequest createCredentialRequest)
    {
        var userKey = createCredentialRequest.UserId.ToString();
        var credentialOptionsBytes = await cache.GetAsync(userKey);
        if (credentialOptionsBytes == null)
        {
            return BadRequest();
        }

        await cache.RemoveAsync(userKey);
        var credentialOptions = CredentialCreateOptions.FromJson(Encoding.UTF8.GetString(credentialOptionsBytes));
        var credential = await fido2.MakeNewCredentialAsync(createCredentialRequest.AttestationResponse,
            credentialOptions,
            (args, _) => userRepository.IsCredentialIdUniqueToUserAsync(args.User.Id, args.CredentialId));
        if (credential.Result != null)
        {
            await userRepository.AddCredentialToUserAsync(
                createCredentialRequest.UserId,
                credential.Result.CredentialId,
                credential.Result.PublicKey,
                credential.Result.Counter,
                (HttpContext.Items[Constants.Device.PlatformInfoKey] as string)!);
            var token = await tokenService.GenerateTokenAsync(createCredentialRequest.UserId);
            return Created("", new CreateCredentialResponse
            {
                CredentialMakeResult = credential,
                Token = token
            });
        }

        return BadRequest();
    }

    [HttpDelete("credential")]
    public async Task<IActionResult> RevokeCredential([FromBody] string encodedCredentialId)
    {
        var credentialId = Convert.FromBase64String(encodedCredentialId);
        var userId = Guid.Parse(User.Claims.First(claim => claim.Type == ClaimConstants.UserId).Value);
        var credentialRevokeResponse = await userRepository.RevokeCredentialAsync(userId, credentialId);
        return credentialRevokeResponse switch
        {
            CredentialRevokeResult.Success => NoContent(),
            CredentialRevokeResult.NotFound => NotFound("No such credential found for user!"),
            CredentialRevokeResult.CannotRevokePrimary => BadRequest("Cannot Revoke the only remaining credential!"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [HttpPost("assertion-options")]
    [AllowAnonymous]
    public async Task<ActionResult<AssertionOptionsResponse>> CreateAssertionOptions([FromBody]string userName, [FromQuery]UserVerificationRequirement userVerificationRequirement = UserVerificationRequirement.Required)
    {
        if (string.IsNullOrEmpty(userName))
        {
            return BadRequest("Username is required to create an authentication options");
        }

        var user = await userRepository.GetUserAsync(userName);
        if (user == null)
        {
            return BadRequest("No such user found");
        }
        
        var existingCredentials = user.Credentials.Select(credential => new PublicKeyCredentialDescriptor(credential.Id));
        var options = fido2.GetAssertionOptions(existingCredentials, userVerificationRequirement);
        await cache.SetAsync(user.Id.ToString(), Encoding.UTF8.GetBytes(options.ToJson()));
        return Ok(new AssertionOptionsResponse
        {
            AssertionOptions = options,
            UserId = user.Id
        });
    }

    [HttpPost("assertion")]
    [AllowAnonymous]
    public async Task<ActionResult<VerifyAssertionResponse>> VerifyAssertion([FromBody] VerifyAssertionRequest verificationRequest)
    {
        var userKey = verificationRequest.UserId.ToString();
        var assertionOptionBytes = await cache.GetAsync(userKey);
        await cache.RemoveAsync(userKey);
        if (assertionOptionBytes == null)
        {
            return BadRequest();
        }
        
        var assertionOptions = AssertionOptions.FromJson(Encoding.UTF8.GetString(assertionOptionBytes));
        var credential = await userRepository.GetCredentialAsync(verificationRequest.AssertionRawResponse.Id);
        var assertionResult = await fido2.MakeAssertionAsync(verificationRequest.AssertionRawResponse, assertionOptions, credential.PublicKey, credential.SignCounter,
            (args, _) => Task.FromResult(new Guid(args.UserHandle) == credential.UserId));
        credential.SignCounter = assertionResult.Counter;
        credential.LastUsedPlatformInfo = HttpContext.Items[Constants.Device.PlatformInfoKey] as string;
        await userRepository.UpdateCredentialAsync(credential);
        var token = await tokenService.GenerateTokenAsync(verificationRequest.UserId);
        return Ok(new VerifyAssertionResponse
        {
            AssertionVerificationResult = assertionResult,
            Token = token
        });
    }
}