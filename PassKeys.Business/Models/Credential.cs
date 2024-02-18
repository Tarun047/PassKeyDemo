namespace PassKeys.Business.Models;

public class Credential : IAuditable
{
    public byte[] Id { get; set; }
    public byte[] PublicKey { get; set; }
    public Guid UserId { get; set; }
    public uint SignCounter { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string LastUsedPlatformInfo { get; set; }
}