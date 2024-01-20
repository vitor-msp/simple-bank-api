namespace Application;

public interface IGetAllAccountsUseCase
{
    Task<GetAllAccountsOutput> Execute();
}