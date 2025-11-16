namespace Auth.Application.Options;

public class AuthOptions
{
    public const string SectionName = "Auth";

    public int AccessTokenMinutes { get; set; } = 5;

    public int RefreshTokenDays { get; set; } = 7;

    public int RefreshTokenLength { get; set; } = 64;
}
