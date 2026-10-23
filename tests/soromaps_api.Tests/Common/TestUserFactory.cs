namespace soromaps_api.Tests.Common;

using soromaps_api.Models;
using soromaps_api.Models.Enums;

/**
 * Fábrica de instâncias de User com valores padrão válidos para cenários de teste.
 */
public static class TestUserFactory
{
    public static User CreateUser(
        Guid? id = null,
        string email = "explorer@soromaps.com",
        string name = "Sorocaba Explorer",
        string passwordHash = "$2a$12$DUMMY_BCRYPT_HASH_FOR_TESTING_PURPOSES",
        UserRole role = UserRole.Explorer)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Email = email,
            Name = name,
            PasswordHash = passwordHash,
            Role = role,
            Neighborhood = "Campolim",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
