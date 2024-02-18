using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PassKeys.Business;
using PassKeys.Business.Models;

namespace PassKeys.WebApp.Controllers;

[Authorize]
[Route("/api/users")]
public class UserController(IUserRepository userRepository) : ControllerBase
{
    [HttpGet("me")]
    public async Task<User> GetCurrentUser()
    {
        return (await userRepository.GetUserAsync(
            User.Claims.First(claim => claim.Type == ClaimConstants.UserName).Value))!;
    }
}