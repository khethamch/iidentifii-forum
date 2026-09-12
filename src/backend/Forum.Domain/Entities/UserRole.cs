namespace Forum.Domain.Entities;

public enum UserRole { Regular, Moderator }

// Preserve the existing Identity role names and JWT claims in SQL Server.
public static class RoleNames
{
    public const string Regular = "User";
    public const string Moderator = "Moderator";
    public static string For(UserRole role) => role switch
    {
        UserRole.Regular => Regular,
        UserRole.Moderator => Moderator,
        _ => throw new ArgumentOutOfRangeException(nameof(role))
    };
}