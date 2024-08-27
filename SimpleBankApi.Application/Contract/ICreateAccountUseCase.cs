using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Application.Exceptions;

public interface ICreateAccountUseCase
{
    Task<CreateAccountOutput> Execute(CreateAccountInput input);
}