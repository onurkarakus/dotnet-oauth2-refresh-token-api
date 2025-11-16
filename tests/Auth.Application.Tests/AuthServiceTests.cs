using Auth.Application.Abstractions.Services;
using Auth.Application.Options;
using Auth.Application.Services;
using Auth.Infrastructure.Options;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Security;
using Auth.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace Auth.Application.Tests;

public class AuthServiceTests
{
    private readonly IAuthService _authService;
    private readonly InMemoryRefreshTokenStore _refreshTokenStore;
    private readonly InMemoryUserService _userService;

    public AuthServiceTests()
    {
        
        var jwtOptions = Microsoft.Extensions.Options.Options.Create(new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            Key = "test-secret-key-must-be-at-least-32-characters-long",
            AccessTokenMinutes = 5,
            RefreshTokenDays = 7
        });

        var authOptions = Microsoft.Extensions.Options.Options.Create(new AuthOptions
        {
            AccessTokenMinutes = 5,
            RefreshTokenDays = 7,
            RefreshTokenLength = 64
        });

        var passwordHasher = new PasswordHasher();
        var jwtTokenGenerator = new JwtTokenGenerator(jwtOptions);

        _refreshTokenStore = new InMemoryRefreshTokenStore();
        _userService = new InMemoryUserService(passwordHasher);

        _authService = new AuthService(
            _userService,
            passwordHasher,
            jwtTokenGenerator,
            _refreshTokenStore,
            authOptions);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var userName = "testuser";
        var password = "Password123!";
        var ipAddress = "127.0.0.1";

        // Act
        var result = await _authService.LoginAsync(userName, password, ipAddress, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        var userName = "testuser";
        var wrongPassword = "WrongPassword";
        var ipAddress = "127.0.0.1";

        // Act
        var result = await _authService.LoginAsync(userName, wrongPassword, ipAddress, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ShouldRotateTokenAndReturnNewTokens()
    {
        // Arrange
        var userName = "testuser";
        var password = "Password123!";
        var ipAddress = "127.0.0.1";

        // Önce login olalım
        var loginResult = await _authService.LoginAsync(userName, password, ipAddress, CancellationToken.None);
        loginResult.Success.Should().BeTrue();

        var oldRefreshToken = loginResult.RefreshToken!;

        // Act
        var refreshResult = await _authService.RefreshAsync(oldRefreshToken, ipAddress, CancellationToken.None);

        // Assert
        refreshResult.Success.Should().BeTrue();
        refreshResult.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshResult.AccessToken.Should().NotBeNullOrWhiteSpace();

        var newRefreshToken = refreshResult.RefreshToken!;

        // Yeni refresh token, eskisi ile aynı olmamalı
        newRefreshToken.Should().NotBe(oldRefreshToken);

        // Eski token artık aktif olmamalı ve revoke edilmiş olmalı
        var existingToken = await _refreshTokenStore.GetAsync(oldRefreshToken);
        existingToken.Should().NotBeNull();
        existingToken!.IsActive.Should().BeFalse();
        existingToken.Revoked.Should().BeTrue();
        existingToken.ReplacedByToken.Should().Be(newRefreshToken);
    }
}
