using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Contract;
using Microsoft.Extensions.Options;
using SimpleBankApi.Domain.Configuration;

namespace SimpleBankApi.Application.UseCases;

public class LoginUseCase : ILoginUseCase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenProvider _tokenProvider;
    private readonly long _refreshTokenExpirationInSeconds;

    public LoginUseCase(IAccountsRepository accountsRepository,
        IPasswordHasher passwordHasher, ITokenProvider tokenProvider,
        IOptions<TokenConfiguration> options)
    {
        _accountsRepository = accountsRepository;
        _passwordHasher = passwordHasher;
        _tokenProvider = tokenProvider;
        _refreshTokenExpirationInSeconds = options.Value.RefreshTokenExpiresInSeconds;
    }

    public async Task<LoginOutput> Execute(LoginInput input)
    {
        var account = await _accountsRepository.GetByAccountNumber(input.AccountNumber);
        if (account == null) throw new EntityNotFoundException("Account number and/or password invalid.");

        var passwordHash = account.GetFields().PasswordHash;
        if (passwordHash == null) throw new EntityNotFoundException("Account number and/or password invalid.");

        var credentialsIsCorrect = _passwordHasher.Verify(passwordHash, input.Password);
        if (!credentialsIsCorrect) throw new EntityNotFoundException("Account number and/or password invalid.");

        var refreshToken = Guid.NewGuid().ToString();
        var refreshTokenExpiration = DateTime.Now.AddSeconds(_refreshTokenExpirationInSeconds);
        account.UpdateRefreshToken(refreshToken, refreshTokenExpiration);
        await _accountsRepository.Save(account);

        return new LoginOutput()
        {
            AccessToken = _tokenProvider.Generate(account),
            RefreshToken = refreshToken
        };
    }
}