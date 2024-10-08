using SimpleBankApi.Application.Exceptions;
using SimpleBankApi.Application.Output;
using SimpleBankApi.Domain.Contract;

namespace SimpleBankApi.Application.UseCases;

public class GetAllAccountsUseCase(IAccountsRepository accountsRepository) : IGetAllAccountsUseCase
{
    private readonly IAccountsRepository _accountsRepository = accountsRepository;

    public async Task<GetAllAccountsOutput> Execute()
    {
        var accounts = await _accountsRepository.GetAll();
        return new GetAllAccountsOutput
        {
            Accounts = accounts.Select(account => AccountDto.Build(account)).ToList()
        };
    }
}