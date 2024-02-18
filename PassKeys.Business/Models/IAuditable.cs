namespace PassKeys.Business.Models;

public interface IAuditable
{
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}