namespace Auth.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public string UserName { get; set; }

    public string PasswordHash { get; set; }

    public string PasswordSalt { get; set; }

    public bool IsActive { get; set; } = true;
}
