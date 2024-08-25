using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Contract;

namespace SimpleBankApi.Application.UseCases;

public class LoginUseCase : ILoginUseCase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenProvider _tokenProvider;

    public LoginUseCase(IAccountsRepository accountsRepository,
        IPasswordHasher passwordHasher, ITokenProvider tokenProvider)
    {
        _accountsRepository = accountsRepository;
        _passwordHasher = passwordHasher;
        _tokenProvider = tokenProvider;
    }

    public async Task<LoginOutput> Execute(LoginInput input)
    {
        var account = await _accountsRepository.GetByAccountNumber(input.AccountNumber);
        var credentialsIsCorrect = _passwordHasher.Verify(account.GetFields().PasswordHash, input.Password);
        if (!credentialsIsCorrect) return default;

        var refreshToken = Guid.NewGuid().ToString();
        account.UpdateRefreshToken(refreshToken);
        await _accountsRepository.Save(account);

        return new LoginOutput()
        {
            AccessToken = _tokenProvider.Generate(account),
            RefreshToken = refreshToken.ToString()
        };
    }
}