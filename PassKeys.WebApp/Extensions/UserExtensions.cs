using Fido2NetLib;
using PassKeys.Business.Models;

namespace PassKeys.WebApp.Extensions;

public static class UserExtensions
{
    public static Fido2User ToFido2User(this User user)
    {
        return new Fido2User
        {
            Id = user.Id.ToByteArray(),
            Name = user.UserName,
            DisplayName = user.DisplayName
        };
    }
}