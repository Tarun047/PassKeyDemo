using Fido2NetLib;

namespace PassKeys.WebApp;

public class VerifyAssertionRequest
{
    public AuthenticatorAssertionRawResponse AssertionRawResponse { get; set; }
    public Guid UserId { get; set; }
}