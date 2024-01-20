using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Input;
using SimpleBankApi.Domain.Contract;
using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Application.UseCases;

public class PostTransferUseCase : IPostTransferUseCase
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;

    public PostTransferUseCase(
        ITransactionsRepository transactionsRepository,
        IAccountsRepository accountsRepository)
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
    }

    public async Task Execute(int accountNumber, TransferInput input)
    {
        Account? sender = await _accountsRepository.GetByAccountNumber(accountNumber);
        if (sender == null) throw new EntityNotFoundException("Sender account not found.");

        Account? recipient = await _accountsRepository.GetByAccountNumber(input.RecipientAccountNumber);
        if (recipient == null) throw new EntityNotFoundException("Recipient account not found.");

        if (sender.Equals(recipient))
            throw new InvalidInputException("Transfer to the same account is not allowed.");

        var calculateBalance = new CalculateBalance(_transactionsRepository);
        double balance = await calculateBalance.FromAccount(sender);
        if (balance < input.Value) throw new InvalidInputException("Insufficient balance.");

        var transfer = new Transfer(input.GetFields()) { Sender = sender, Recipient = recipient };
        await _transactionsRepository.SaveTransfer(transfer);
    }
}