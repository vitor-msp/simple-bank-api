using Models;

namespace Application;

public class GetAllAccountsUseCase : IGetAllAccountsUseCase
{
    private readonly IAccountsRepository _accountsRepository;

    public GetAllAccountsUseCase(IAccountsRepository accountsRepository)
    {
        _accountsRepository = accountsRepository;
    }

    public async Task<GetAllAccountsOutput> Execute()
    {
        var accounts = await _accountsRepository.GetAll();
        return new GetAllAccountsOutput
        {
            Accounts = accounts.Select(a => new TransactionAccountDto(a.GetPublicData())).ToList()
        };
    }
}