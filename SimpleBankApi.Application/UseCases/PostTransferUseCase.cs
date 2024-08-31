using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Utils;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Application.UseCases;

public class PostTransferUseCase : IPostTransferUseCase
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;
    private readonly ICalculateBalance _calculateBalance;
    private readonly IBankCache _bankCache;

    public PostTransferUseCase(
        ITransactionsRepository transactionsRepository,
        IAccountsRepository accountsRepository,
        ICalculateBalance calculateBalance,
        IBankCache bankCache)
    {
        _accountsRepository = accountsRepository;
        _transactionsRepository = transactionsRepository;
        _calculateBalance = calculateBalance;
        _bankCache = bankCache;
    }

    public async Task Execute(int accountNumber, TransferInput input)
    {
        var sender = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (sender == null) throw new EntityNotFoundException("Sender account not found.");

        var recipient = await _accountsRepository.GetByAccountNumber(input.RecipientAccountNumber);
        if (recipient == null) throw new EntityNotFoundException("Recipient account not found.");

        if (sender.Equals(recipient))
            throw new InvalidInputException("Transfer to the same account is not allowed.");

        double balance = await _calculateBalance.FromAccount(sender);
        if (balance < input.Value) throw new InvalidInputException("Insufficient balance.");

        var transfer = input.GetTransfer(sender, recipient);
        await _transactionsRepository.SaveTransfer(transfer);

        await _bankCache.Delete(CacheKeys.Balance(sender));
        await _bankCache.Delete(CacheKeys.Balance(recipient));
    }
}