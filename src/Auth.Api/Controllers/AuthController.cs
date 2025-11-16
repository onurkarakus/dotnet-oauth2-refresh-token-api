using Auth.Application.Abstractions.Services;
using Auth.Application.Services;
using Auth.Domain.RequestEntities;
using Auth.Domain.ResponseEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Auth.Api.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;
    private readonly ILogger<AuthController> logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        this.authService = authService;
        this.logger = logger;
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "UserName and Password are required." });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var result = await authService.LoginAsync(request.UserName, request.Password, ipAddress, cancellationToken);

        if (!result.Success)
        {
            logger.LogWarning("Failed login attempt for user {UserName} from IP {IpAddress}", request.UserName, ipAddress);

            return Unauthorized(new { error = result.Error });
        }

        var response = new AuthResponse
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiresIn = result.ExpiresIn ?? 0
        };

        return Ok(response);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { error = "RefreshToken is required." });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var result = await authService.RefreshAsync(request.RefreshToken, ipAddress, cancellationToken);

        if (!result.Success)
        {
            logger.LogWarning("Failed refresh attempt from IP {IpAddress}", ipAddress);

            return Unauthorized(new { error = result.Error });
        }

        var response = new AuthResponse
        {
            AccessToken = result.AccessToken!,
            RefreshToken = result.RefreshToken!,
            ExpiresIn = result.ExpiresIn ?? 0
        };

        return Ok(response);
    }
}
