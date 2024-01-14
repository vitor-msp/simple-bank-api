namespace Models;

public interface ITransactionsRepository
{
    Task SaveCredit(Credit credit);
    Task SaveDebit(Debit debit);
    Task SaveTransfer(Transfer transfer);
    Task<List<Credit>> GetAccountCredits(int accountNumber);
    Task<List<Debit>> GetAccountDebits(int accountNumber);
    Task<List<Transfer>> GetAccountTransfers(int accountNumber);
}