using Fido2NetLib;

namespace PassKeys.WebApp.Authentication.Models;

public class CreateCredentialResponse
{
    public Fido2.CredentialMakeResult CredentialMakeResult { get; set; }
    public string Token { get; set; }
}