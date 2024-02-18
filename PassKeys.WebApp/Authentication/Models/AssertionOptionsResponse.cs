using Fido2NetLib;

namespace PassKeys.WebApp;

public class AssertionOptionsResponse
{
    public AssertionOptions AssertionOptions { get; set; }
    public Guid UserId { get; set; }
}