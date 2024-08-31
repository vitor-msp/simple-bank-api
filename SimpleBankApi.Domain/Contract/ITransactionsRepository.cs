using SimpleBankApi.Domain.Entities;
using SimpleBankApi.Domain.ValueObjects;

namespace SimpleBankApi.Domain.Contract;

public interface ITransactionsRepository
{
    Task SaveCredit(ICredit credit);
    Task SaveDebit(IDebit debit);
    Task SaveTransfer(ITransfer transfer);
    Task<List<TransactionWrapper>> GetTransactionsFromAccount(int accountNumber);
}