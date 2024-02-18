using Fido2NetLib.Development;
using PassKeys.Business.Models;

namespace PassKeys.Business;

public interface IUserRepository
{
    Task<User> CreateUserAsync(string userName);

    Task<User?> GetUserAsync(string userName);
    ValueTask<User?> GetUserAsync(Guid userId);

    ValueTask<Credential?> GetCredentialAsync(byte[] credentialId);

    Task<bool> IsCredentialIdUniqueToUserAsync(byte[] userHandle, byte[] credentialId);

    Task<bool> VerifyCredentialOwnershipAsync(byte[] userHandle, byte[] credentialId);

    Task UpdateCredentialAsync(Credential credential);

    Task AddCredentialToUserAsync(Guid userId, byte[] credentialId, byte[] publicKey, uint signCounter, string lastUsedPlatformInfo);

    Task<CredentialRevokeResult> RevokeCredentialAsync(Guid userId, byte[] credentialId);
}