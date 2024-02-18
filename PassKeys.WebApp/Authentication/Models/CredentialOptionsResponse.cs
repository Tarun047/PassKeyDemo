using Fido2NetLib;
using PassKeys.Business.Models;

namespace PassKeys.WebApp;

public class CredentialOptionsResponse
{
    public CredentialCreateOptions Options { get; set; }
    public Guid UserId { get; set; }
}