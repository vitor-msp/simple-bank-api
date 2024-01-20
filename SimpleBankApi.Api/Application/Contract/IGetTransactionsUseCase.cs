namespace Application;

public interface IGetTransactionsUseCase
{
    Task<GetTransactionsOutput> Execute(int accountNumber);
}