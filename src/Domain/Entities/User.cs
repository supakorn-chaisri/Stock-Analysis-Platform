using Microsoft.AspNetCore.Identity;
using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public UserTier Tier { get; private set; } = UserTier.Free;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private User() { } // EF Core

    public static User Create(string email, string passwordHash, UserTier tier = UserTier.Free)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            UserName = email.ToLowerInvariant(),
            Email = email.ToLowerInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            NormalizedEmail = email.ToUpperInvariant(),
            PasswordHash = passwordHash,
            Tier = tier,
            EmailConfirmed = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void VerifyEmail()
    {
        EmailConfirmed = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpgradeToPremium()
    {
        Tier = UserTier.Premium;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}