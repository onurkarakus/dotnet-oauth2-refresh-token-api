using Auth.Application.Abstractions.Persistence;
using Auth.Application.Abstractions.Security;
using Auth.Application.Abstractions.Services;
using Auth.Application.Options;
using Auth.Domain.Entities;
using Auth.Domain.Models.Auth;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace Auth.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserService userService;
    private readonly IPasswordHasher passwordHasher;
    private readonly IJwtTokenGenerator jwtTokenGenerator;
    private readonly IRefreshTokenStore refreshTokenStore;
    private readonly AuthOptions authOptions;

    public AuthService(
        IUserService userService,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenStore refreshTokenStore,
        IOptions<AuthOptions> authOptions)
    {
        this.userService = userService;
        this.passwordHasher = passwordHasher;
        this.jwtTokenGenerator = jwtTokenGenerator;
        this.refreshTokenStore = refreshTokenStore;
        this.authOptions = authOptions.Value;
    }

    public async Task<AuthResult> LoginAsync(string userName, string password, string ipAddress, CancellationToken cancellationToken)
    {
        var user = await userService.GetByUserNameAsync(userName, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return AuthResult.Fail("Invalid username or password.");
        }

        var isPasswordValid = passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);

        if (!isPasswordValid)
        {
            return AuthResult.Fail("Invalid username or password.");
        }

        var accessToken = jwtTokenGenerator.GenerateAccessToken(user);

        var now = DateTime.UtcNow;
        var refreshToken = CreateRefreshToken(user.Id, now, ipAddress);

        await refreshTokenStore.StoreAsync(refreshToken, cancellationToken);

        var expiresInSeconds = authOptions.AccessTokenMinutes * 60;

        return AuthResult.Ok(accessToken, refreshToken.Token, expiresInSeconds);

    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var existingToken = await refreshTokenStore.GetAsync(refreshToken, cancellationToken);

        if (existingToken is null)
        {
            return AuthResult.Fail("Invalid refresh token.");
        }

        if (!existingToken.IsActive)
        {
            if (existingToken.Revoked && now > existingToken.ExpiresAt)
            {
                existingToken.Revoked = true;
                existingToken.RevokedAt = now;
                existingToken.RevokedByIp = ipAddress;
                existingToken.ReasonRevoked = "Expired.";

                await refreshTokenStore.UpdateAsync(existingToken, cancellationToken);
            }

            return AuthResult.Fail("Refresh token is no longer valid.");
        }

        var user = await userService.GetByIdAsync(existingToken.UserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return AuthResult.Fail("User not found or inactive.");
        }

        var newRefreshToken = CreateRefreshToken(user.Id, now, ipAddress);

        existingToken.Revoked = true;
        existingToken.RevokedAt = now;
        existingToken.RevokedByIp = ipAddress;
        existingToken.ReplacedByToken = newRefreshToken.Token;
        existingToken.ReasonRevoked = "Replaced by new token.";
        await refreshTokenStore.UpdateAsync(existingToken, cancellationToken);
        await refreshTokenStore.StoreAsync(newRefreshToken, cancellationToken);

        var accesToken = jwtTokenGenerator.GenerateAccessToken(user);
        var expiresInSeconds = authOptions.AccessTokenMinutes * 60;

        return AuthResult.Ok(accesToken, newRefreshToken.Token, expiresInSeconds);
    }

    private RefreshToken CreateRefreshToken(Guid userId, DateTime now, string ipAddress)
    {
        var lenght = authOptions.RefreshTokenLength;
        var randomBytes = RandomNumberGenerator.GetBytes(lenght);
        var token = Convert.ToBase64String(randomBytes);

        return new RefreshToken
        {
            Token = token,
            UserId = userId,
            CreatedAt = now,
            ExpiresAt = now.AddDays(authOptions.RefreshTokenDays),
            Revoked = false,
            RevokedByIp = ipAddress,
            RevokedAt = null,
            ReplacedByToken = null,
            ReasonRevoked = null
        };
    }
}
