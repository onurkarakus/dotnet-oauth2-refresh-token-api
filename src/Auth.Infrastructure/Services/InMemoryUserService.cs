using Auth.Application.Abstractions.Security;
using Auth.Application.Abstractions.Services;
using Auth.Domain.Entities;

namespace Auth.Infrastructure.Services;

public class InMemoryUserService : IUserService
{
    private readonly List<User> _users = new();

    public InMemoryUserService(IPasswordHasher passwordHasher)
    {
        var (hash, salt) = passwordHasher.HasPassword("Password123!");

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsActive = true
        };

        _users.Add(user);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = _users.Find(u => u.Id == id);

        return Task.FromResult(user);
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken)
    {
        var user = _users.Find(u =>
            string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user);
    }
}
