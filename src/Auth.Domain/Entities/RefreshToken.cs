namespace Auth.Domain.Entities;

public class RefreshToken
{
    public string Token { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool Revoked { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? RevokedByIp { get; set; }

    public string? ReplacedByToken { get; set; }

    public string? ReasonRevoked { get; set; }

    public bool IsActive => !Revoked && DateTime.UtcNow <= ExpiresAt;
}
