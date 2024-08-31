using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Utils;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Application.UseCases;

public class PostTransferUseCase(
    ITransactionsRepository transactionsRepository,
    IAccountsRepository accountsRepository,
    ICalculateBalance calculateBalance,
    IBankCache bankCache) : IPostTransferUseCase
{
    private readonly ITransactionsRepository _transactionsRepository = transactionsRepository;
    private readonly IAccountsRepository _accountsRepository = accountsRepository;
    private readonly ICalculateBalance _calculateBalance = calculateBalance;
    private readonly IBankCache _bankCache = bankCache;

    public async Task Execute(int accountNumber, TransferInput input)
    {
        var sender = await _accountsRepository.GetByAccountNumber(accountNumber)
            ?? throw new EntityNotFoundException("Sender account not found.");

        var recipient = await _accountsRepository.GetByAccountNumber(input.RecipientAccountNumber)
            ?? throw new EntityNotFoundException("Recipient account not found.");

        if (sender.Equals(recipient))
            throw new InvalidInputException("Transfer to the same account is not allowed.");

        var senderCacheKey = CacheKeys.Balance(sender);
        var balanceCacheValue = await _bankCache.Get(senderCacheKey);

        if (balanceCacheValue == null || !double.TryParse(balanceCacheValue, out double balance))
            balance = await _calculateBalance.FromAccount(sender);

        if (balance < input.Value) throw new InvalidInputException("Insufficient balance.");

        var transfer = input.GetTransfer(sender, recipient);
        await _transactionsRepository.SaveTransfer(transfer);

        await _bankCache.Delete(senderCacheKey);
        await _bankCache.Delete(CacheKeys.Balance(recipient));
    }
}