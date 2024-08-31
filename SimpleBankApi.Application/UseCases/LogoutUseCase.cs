using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Domain.Contract;

namespace SimpleBankApi.Application.UseCases;

public class LogoutUseCase(IAccountsRepository accountsRepository) : ILogoutUseCase
{
    private readonly IAccountsRepository _accountsRepository = accountsRepository;

    public async Task Execute(LogoutInput input)
    {
        var account = await _accountsRepository.GetByAccountNumber(input.AccountNumber)
            ?? throw new EntityNotFoundException("Account number and/or refresh token invalid.");

        var refreshToken = account.RefreshToken;
        if (refreshToken == null || !refreshToken.Equals(input.RefreshToken))
            throw new EntityNotFoundException("Account number and/or refresh token invalid.");

        var refreshTokenExpiration = account.RefreshTokenExpiration;
        if (refreshTokenExpiration == null || DateTime.Now >= refreshTokenExpiration)
            throw new EntityNotFoundException("Account number and/or refresh token invalid.");

        account.UpdateRefreshToken(null, null);
        await _accountsRepository.Save(account);
    }
}