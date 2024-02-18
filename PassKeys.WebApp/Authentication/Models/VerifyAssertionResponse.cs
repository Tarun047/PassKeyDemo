using Fido2NetLib.Objects;

namespace PassKeys.WebApp.Authentication.Models;

public class VerifyAssertionResponse
{
    public AssertionVerificationResult AssertionVerificationResult { get; set; }
    public string Token { get; set; }
}