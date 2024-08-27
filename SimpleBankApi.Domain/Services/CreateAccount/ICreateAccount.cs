namespace SimpleBankApi.Domain.Services;

public interface ICreateAccount
{
    Task<CreateAccountOutput> Execute(CreateAccountInput input);
}