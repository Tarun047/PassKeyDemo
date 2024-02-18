using Fido2NetLib;

namespace PassKeys.WebApp;

public class CreateCredentialRequest
{
    public AuthenticatorAttestationRawResponse AttestationResponse { get; set; }
    public Guid UserId { get; set; }
}