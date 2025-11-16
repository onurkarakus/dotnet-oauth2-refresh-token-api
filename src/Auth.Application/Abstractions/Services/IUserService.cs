using Auth.Domain.Entities;

namespace Auth.Application.Abstractions.Services;

public interface IUserService
{
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
