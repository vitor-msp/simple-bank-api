using Dto;

namespace Application;

public interface IPostTransferUseCase
{
    Task Execute(int accountNumber, TransferDto transferDto);
}