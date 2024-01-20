using SimpleBankApi.Domain.Entities;

namespace SimpleBankApi.Domain.Contract;

public interface ITransactionsRepository
{
    Task SaveCredit(Credit credit);
    Task SaveDebit(Debit debit);
    Task SaveTransfer(Transfer transfer);
    Task<List<Credit>> GetCreditsFromAccount(int accountNumber);
    Task<List<Debit>> GetDebitsFromAccount(int accountNumber);
    Task<List<Transfer>> GetTransfersFromAccount(int accountNumber);
}