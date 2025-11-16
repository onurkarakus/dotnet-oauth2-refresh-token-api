namespace Auth.Application.Abstractions.Security;

public interface IPasswordHasher
{
    (string Hash, string Salt) HasPassword(string password);

    bool VerifyPassword(string password, string hash, string salt);
}
