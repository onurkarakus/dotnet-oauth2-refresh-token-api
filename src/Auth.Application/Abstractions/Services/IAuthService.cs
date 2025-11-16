using Auth.Domain.Models.Auth;

namespace Auth.Application.Abstractions.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string userName, string password, string ipAddress, CancellationToken cancellationToken);

    Task<AuthResult> RefreshAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken);
}
