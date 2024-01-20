namespace Application;

public interface IGetBalanceUseCase
{
    Task<GetBalanceOutput> Execute(int accountNumber);
}