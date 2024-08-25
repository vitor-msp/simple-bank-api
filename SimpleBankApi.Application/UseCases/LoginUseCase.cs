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
        if (account == null) throw new EntityNotFoundException("Account number and/or password invalid.");

        var passwordHash = account.GetFields().PasswordHash;
        if (passwordHash == null) throw new EntityNotFoundException("Account number and/or password invalid.");

        var credentialsIsCorrect = _passwordHasher.Verify(passwordHash, input.Password);
        if (!credentialsIsCorrect) throw new EntityNotFoundException("Account number and/or password invalid.");

        var refreshToken = Guid.NewGuid().ToString();
        account.UpdateRefreshToken(refreshToken);
        await _accountsRepository.Save(account);

        return new LoginOutput()
        {
            AccessToken = _tokenProvider.Generate(account),
            RefreshToken = refreshToken
        };
    }
}