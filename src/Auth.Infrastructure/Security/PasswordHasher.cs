using Auth.Application.Abstractions.Security;
using System.Security.Cryptography;

namespace Auth.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32; // 256 bit
    private const int Iterations = 100_000;

    public (string Hash, string Salt) HasPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);

        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        var hash = Convert.ToBase64String(keyBytes);
        var salt = Convert.ToBase64String(saltBytes);

        return (hash, salt);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Convert.FromBase64String(hash);

        var keyBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return CryptographicOperations.FixedTimeEquals(keyBytes, hashBytes);
    }
}
