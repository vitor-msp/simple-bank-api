using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Contract;

namespace SimpleBankApi.Application.UseCases;

public class RefreshTokenUseCase : IRefreshTokenUseCase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly ITokenProvider _tokenProvider;

    public RefreshTokenUseCase(IAccountsRepository accountsRepository, ITokenProvider tokenProvider)
    {
        _accountsRepository = accountsRepository;
        _tokenProvider = tokenProvider;
    }

    public async Task<RefreshTokenOutput> Execute(RefreshTokenInput input)
    {
        var account = await _accountsRepository.GetByAccountNumber(input.AccountNumber);
        if (account == null) throw new EntityNotFoundException("Account number and/or password invalid.");

        return new RefreshTokenOutput()
        {
            AccessToken = _tokenProvider.Generate(account)
        };
    }
}