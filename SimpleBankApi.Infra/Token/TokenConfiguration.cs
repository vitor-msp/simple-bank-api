namespace SimpleBankApi.Infra;

public class TokenConfiguration
{
    public string? Key { get; init; }
    public int AccessTokenExpiresInSeconds { get; init; }
    public int RefreshTokenExpiresInSeconds { get; init; }
}