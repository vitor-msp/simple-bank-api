using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Domain.Entities;

public interface IAccount
{
    int Id { get; }
    int AccountNumber { get; }
    DateTime CreatedAt { get; }
    bool Active { get; }
    Role Role { get; set; }
    ICustomer Owner { get; set; }
    string PasswordHash { get; set; }
    string? RefreshToken { get; }
    DateTime? RefreshTokenExpiration { get; }
    void Inactivate();
    void UpdateRefreshToken(string? token, DateTime? expiration);
}