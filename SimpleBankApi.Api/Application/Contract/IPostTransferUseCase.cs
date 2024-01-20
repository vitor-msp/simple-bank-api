using Input;

namespace Application;

public interface IPostTransferUseCase
{
    Task Execute(int accountNumber, TransferInput transferDto);
}