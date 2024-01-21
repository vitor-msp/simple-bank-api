using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.Contract;

public interface ITransactionsRepository
{
    Task SaveCredit(ICredit credit);
    Task SaveDebit(IDebit debit);
    Task SaveTransfer(ITransfer transfer);
    Task<List<ICredit>> GetCreditsFromAccount(int accountNumber);
    Task<List<IDebit>> GetDebitsFromAccount(int accountNumber);
    Task<List<ITransfer>> GetTransfersFromAccount(int accountNumber);
}