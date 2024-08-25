using SimpleBankApi.Application.Input;
using SimpleBankApi.Application.Output;

namespace SimpleBankApi.Application.Exceptions;

public interface IRefreshTokenUseCase
{
    Task<RefreshTokenOutput> Execute(RefreshTokenInput input);
}