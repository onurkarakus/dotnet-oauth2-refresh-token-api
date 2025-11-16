using Auth.Domain.RequestEntities;
using Auth.Domain.ResponseEntities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Auth.Api.Tests;

public class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithTokens()
    {
        // Arrange
        var request = new LoginRequest
        {
            UserName = "testuser",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.AccessToken.Should().NotBeNullOrWhiteSpace();
        authResponse.RefreshToken.Should().NotBeNullOrWhiteSpace();
        authResponse.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            UserName = "testuser",
            Password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_TooManyRequests_ShouldEventuallyReturnTooManyRequests()
    {
        var request = new LoginRequest
        {
            UserName = "testuser",
            Password = "WrongPassword"
        };

        const int attempts = 6;
        HttpResponseMessage? lastResponse = null;

        for (int i = 0; i < attempts; i++)
        {
            lastResponse = await _client.PostAsJsonAsync("/auth/login", request);
        }

        lastResponse.Should().NotBeNull();
        lastResponse!.StatusCode.Should().BeOneOf(HttpStatusCode.TooManyRequests, HttpStatusCode.ServiceUnavailable);
    }
}
