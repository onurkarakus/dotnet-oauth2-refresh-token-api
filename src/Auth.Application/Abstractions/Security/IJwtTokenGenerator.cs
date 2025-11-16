using Auth.Domain.Entities;
using System.Security.Claims;

namespace Auth.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user, IEnumerable<Claim>? additionalClaims = null);
}
