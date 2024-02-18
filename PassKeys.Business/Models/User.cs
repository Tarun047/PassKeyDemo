namespace PassKeys.Business.Models;

public class User
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }

    public IEnumerable<Credential> Credentials { get; set; }
}