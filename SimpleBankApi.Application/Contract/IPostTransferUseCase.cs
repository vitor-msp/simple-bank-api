using SimpleBankApi.Application.Input;

namespace SimpleBankApi.Application.Exceptions;

public interface IPostTransferUseCase
{
    Task Execute(int accountNumber, TransferInput input);
}