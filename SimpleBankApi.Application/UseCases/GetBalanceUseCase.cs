using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Extensions;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Application.UseCases;

public class GetBalanceUseCase : IGetBalanceUseCase
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly ICalculateBalance _calculateBalance;

    public GetBalanceUseCase(
        IAccountsRepository accountsRepository,
        ICalculateBalance calculateBalance)
    {
        _accountsRepository = accountsRepository;
        _calculateBalance = calculateBalance;
    }

    public async Task<GetBalanceOutput> Execute(int accountNumber)
    {
        var account = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (account == null) throw new EntityNotFoundException("Account not found.");

        double balance = await _calculateBalance.FromAccount(account);

        return new GetBalanceOutput { Balance = balance.GetBrazilianCurrency() };
    }
}