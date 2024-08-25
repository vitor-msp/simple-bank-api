using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Domain.Contract;

namespace SimpleBankApi.Application.UseCases;

public class LogoutUseCase : ILogoutUseCase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LogoutUseCase(IAccountsRepository accountsRepository, IPasswordHasher passwordHasher)
    {
        _accountsRepository = accountsRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task Execute(LogoutInput input)
    {
        var account = await _accountsRepository.GetByAccountNumber(input.AccountNumber);
        if (account == null) throw new EntityNotFoundException("Account number and/or refresh token invalid.");

        var refreshToken = account.GetFields().RefreshToken;
        if (refreshToken == null || !refreshToken.Equals(input.RefreshToken))
            throw new EntityNotFoundException("Account number and/or refresh token invalid.");

        var refreshTokenExpiration = account.GetFields().RefreshTokenExpiration;
        if (refreshTokenExpiration == null || DateTime.Now >= refreshTokenExpiration)
            throw new EntityNotFoundException("Account number and/or refresh token invalid.");

        account.UpdateRefreshToken("");
        await _accountsRepository.Save(account);
    }
}