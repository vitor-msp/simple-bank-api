using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Contract;

namespace SimpleBankApi.Application.UseCases;

public class RefreshTokenUseCase(IAccountsRepository accountsRepository,
    ITokenProvider tokenProvider) : IRefreshTokenUseCase
{
    private readonly IAccountsRepository _accountsRepository = accountsRepository;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    public async Task<RefreshTokenOutput> Execute(RefreshTokenInput input)
    {
        var account = await _accountsRepository.GetByAccountNumber(input.AccountNumber)
            ?? throw new EntityNotFoundException("Account number and/or refresh token invalid.");

        var refreshToken = account.RefreshToken;
        if (refreshToken == null || !refreshToken.Equals(input.RefreshToken))
            throw new EntityNotFoundException("Account number and/or refresh token invalid.");

        var refreshTokenExpiration = account.RefreshTokenExpiration;
        if (refreshTokenExpiration == null || DateTime.Now >= refreshTokenExpiration)
            throw new EntityNotFoundException("Account number and/or refresh token invalid.");

        return new RefreshTokenOutput()
        {
            AccessToken = _tokenProvider.Generate(account)
        };
    }
}