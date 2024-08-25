namespace SimpleBankApi.Domain.Configuration;

public class TokenConfiguration
{
    public string? Key { get; init; }
    public long AccessTokenExpiresInSeconds { get; init; }
    public long RefreshTokenExpiresInSeconds { get; init; }
}