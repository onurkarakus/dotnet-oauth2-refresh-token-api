using Auth.Application.Abstractions.Persistence;
using Auth.Domain.Entities;
using System.Collections.Concurrent;

namespace Auth.Infrastructure.Persistence;

public class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    private readonly ConcurrentDictionary<string, RefreshToken> _tokens = new();

    public Task<RefreshToken?> GetAsync(string token, CancellationToken cancellationToken = default)
    {
        _tokens.TryGetValue(token, out var refreshToken);

        return Task.FromResult(refreshToken);
    }

    public Task StoreAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _tokens[refreshToken.Token] = refreshToken;

        return Task.CompletedTask;
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _tokens[refreshToken.Token] = refreshToken;

        return Task.CompletedTask;
    }
}
