using SimpleBankApi.Domain.Services;

namespace SimpleBankApi.Application.Exceptions;

public interface ICreateAdminAccountUseCase
{
    Task<CreateAccountOutput> Execute(CreateAccountInput input);
}