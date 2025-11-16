using Auth.Domain.Entities;

namespace Auth.Application.Abstractions.Persistence;

public interface IRefreshTokenStore
{
    Task StoreAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetAsync(string token, CancellationToken cancellationToken = default);

    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
