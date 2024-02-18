using Fido2NetLib.Development;
using Microsoft.EntityFrameworkCore;
using PassKeys.Business.Models;

namespace PassKeys.Business;

public class UserRepository(PassKeysDbContext dbContext) : IUserRepository
{
    public async Task<User> CreateUserAsync(string userName)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Credentials = new List<Credential>(),
            DisplayName = userName
        };

        var userEntity = await dbContext.AddAsync(user);
        await dbContext.SaveChangesAsync();
        return userEntity.Entity;
    }

    public async Task<User?> GetUserAsync(string userName)
    {
        return await dbContext.Users.Where(user => user.UserName == userName).Include(user => user.Credentials)
            .FirstOrDefaultAsync();
    }

    public async ValueTask<User?> GetUserAsync(Guid userId)
    {
        return await dbContext.Users.FindAsync(userId);
    }

    public async ValueTask<Credential?> GetCredentialAsync(byte[] credentialId)
    {
        return await dbContext.Credentials.FindAsync(credentialId);
    }

    public async Task<bool> IsCredentialIdUniqueToUserAsync(byte[] userHandle, byte[] credentialId)
    {
        var userId = new Guid(userHandle);
        return (await dbContext.Credentials.LongCountAsync(credential => credential.UserId == userId && credential.Id == credentialId) == 0);
    }

    public async Task<bool> VerifyCredentialOwnershipAsync(byte[] userHandle, byte[] credentialId)
    {
        var userId = new Guid(userHandle);
        var credential = await dbContext.Credentials.FindAsync(credentialId);
        if (credential != null)
        {
            return credential.UserId == userId;
        }

        return false;
    }

    public async Task UpdateCredentialAsync(Credential credential)
    {
        dbContext.Update(credential);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddCredentialToUserAsync(Guid userId, byte[] credentialId, byte[] publicKey, uint signCounter, string lastUsedPlatformInfo)
    {
        await dbContext.AddAsync(new Credential
        {
            Id = credentialId,
            PublicKey = publicKey,
            SignCounter = signCounter,
            LastUsedPlatformInfo = lastUsedPlatformInfo,
            UserId = userId
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task<CredentialRevokeResult> RevokeCredentialAsync(Guid userId, byte[] credentialId)
    {
        var otherCredentialCount = await dbContext.Credentials.Where(credential => credential.UserId == userId && credential.Id != credentialId).LongCountAsync();
        if (otherCredentialCount > 0)
        {
            var affectedRows = await dbContext.Credentials.Where(
                credential => credential.UserId == userId && credential.Id == credentialId).ExecuteDeleteAsync();
            return affectedRows > 0 ? CredentialRevokeResult.Success : CredentialRevokeResult.NotFound;
        }

        return CredentialRevokeResult.CannotRevokePrimary;
    }
}