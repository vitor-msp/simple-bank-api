using Application;
using Application.Exceptions;
using Exceptions;
using Input;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController : ControllerBase
{
    private readonly IPostCreditUseCase _postCreditUseCase;
    private readonly IPostDebitUseCase _postDebitUseCase;
    private readonly IPostTransferUseCase _postTransferUseCase;
    private readonly IGetBalanceUseCase _getBalanceUseCase;
    private readonly IGetTransactionsUseCase _getTransactionsUseCase;

    public TransactionsController(
        IPostCreditUseCase postCreditUseCase,
        IPostDebitUseCase postDebitUseCase,
        IPostTransferUseCase postTransferUseCase,
        IGetBalanceUseCase getBalanceUseCase,
        IGetTransactionsUseCase getTransactionsUseCase
        )
    {
        _postCreditUseCase = postCreditUseCase;
        _postDebitUseCase = postDebitUseCase;
        _postTransferUseCase = postTransferUseCase;
        _getBalanceUseCase = getBalanceUseCase;
        _getTransactionsUseCase = getTransactionsUseCase;
    }

    [HttpPost("credit/{accountNumber}")]
    public async Task<ActionResult> PostCredit(int accountNumber, [FromBody] CreditInput creditDto)
    {
        try
        {
            await _postCreditUseCase.Execute(accountNumber, creditDto);
            return Ok();
        }
        catch (EntityNotFoundException error)
        {
            return NotFound(new ErrorDto(error.Message));
        }
        catch (DomainException error)
        {
            return BadRequest(new ErrorDto(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to create credit."));
        }
    }

    [HttpPost("debit/{accountNumber}")]
    public async Task<ActionResult> PostDebit(int accountNumber, [FromBody] DebitInput debitDto)
    {
        try
        {
            await _postDebitUseCase.Execute(accountNumber, debitDto);
            return Ok();
        }
        catch (EntityNotFoundException error)
        {
            return NotFound(new ErrorDto(error.Message));
        }
        catch (InvalidInputException error)
        {
            return BadRequest(new ErrorDto(error.Message));
        }
        catch (DomainException error)
        {
            return BadRequest(new ErrorDto(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to create debit."));
        }
    }

    [HttpPost("transfer/{accountNumber}")]
    public async Task<ActionResult> PostTransfer(int accountNumber, [FromBody] TransferInput transferDto)
    {
        try
        {
            await _postTransferUseCase.Execute(accountNumber, transferDto);
            return Ok();
        }
        catch (EntityNotFoundException error)
        {
            return NotFound(new ErrorDto(error.Message));
        }
        catch (InvalidInputException error)
        {
            return BadRequest(new ErrorDto(error.Message));
        }
        catch (DomainException error)
        {
            return BadRequest(new ErrorDto(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to create transfer."));
        }
    }

    [HttpGet("balance/{accountNumber}")]
    public async Task<ActionResult<GetBalanceOutput>> GetBalance(int accountNumber)
    {
        try
        {
            var output = await _getBalanceUseCase.Execute(accountNumber);
            return Ok(output);
        }
        catch (EntityNotFoundException error)
        {
            return NotFound(new ErrorDto(error.Message));

        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to get balance."));
        }
    }

    [HttpGet("{accountNumber}")]
    public async Task<ActionResult<GetTransactionsOutput>> GetTransactions(int accountNumber)
    {
        try
        {
            var output = await _getTransactionsUseCase.Execute(accountNumber);
            return Ok(output);
        }
        catch (EntityNotFoundException error)
        {
            return NotFound(new ErrorDto(error.Message));

        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to get transactions."));
        }
    }
}