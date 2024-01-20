using System.Collections;
using Application;
using Application.Exceptions;
using Dto;
using Exceptions;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Controllers;

[ApiController]
[Route("transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly IAccountsRepository _accountsRepository;
    private readonly IPostCreditUseCase _postCreditUseCase;
    private readonly IPostDebitUseCase _postDebitUseCase;
    private readonly IPostTransferUseCase _postTransferUseCase;
    private readonly IGetBalanceUseCase _getBalanceUseCase;

    public TransactionsController(
        ITransactionsRepository transactionsRepository,
        IAccountsRepository accountsRepository,
        IPostCreditUseCase postCreditUseCase,
        IPostDebitUseCase postDebitUseCase,
        IPostTransferUseCase postTransferUseCase,
        IGetBalanceUseCase getBalanceUseCase
        )
    {
        _transactionsRepository = transactionsRepository;
        _accountsRepository = accountsRepository;
        _postCreditUseCase = postCreditUseCase;
        _postDebitUseCase = postDebitUseCase;
        _postTransferUseCase = postTransferUseCase;
        _getBalanceUseCase = getBalanceUseCase;
    }

    [HttpPost("credit/{accountNumber}")]
    public async Task<ActionResult> PostCredit(int accountNumber, [FromBody] CreditDto creditDto)
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
        catch (TransactionException error)
        {
            return BadRequest(new ErrorDto(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to create credit."));
        }
    }

    [HttpPost("debit/{accountNumber}")]
    public async Task<ActionResult> PostDebit(int accountNumber, [FromBody] DebitDto debitDto)
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
        catch (TransactionException error)
        {
            return BadRequest(new ErrorDto(error.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorDto("Error to create debit."));
        }
    }

    [HttpPost("transfer/{accountNumber}")]
    public async Task<ActionResult> PostTransfer(int accountNumber, [FromBody] TransferDto transferDto)
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
        catch (TransactionException error)
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

    public class GetTransactionsOutput
    {
        public ArrayList Transactions { get; set; } = new();
    }

    [HttpGet("{accountNumber}")]
    public async Task<ActionResult<GetTransactionsOutput>> GetTransactions(int accountNumber)
    {
        try
        {
            Account? account = await _accountsRepository.GetByAccountNumber(accountNumber);
            if (account == null) return NotFound(new ErrorDto("Account not found."));
            var credits = await GetCreditsFromAccount(account);
            var debits = await GetDebitsFromAccount(account);
            var transfers = await GetTransfersFromAccount(account);
            var sortedTransactions = SortTransactionsByDateTime(credits, debits, transfers, account);
            return Ok(new GetTransactionsOutput { Transactions = sortedTransactions });
        }
        catch (Exception error)
        {
            System.Console.WriteLine(error.Message);
            System.Console.WriteLine(error.StackTrace);
            return StatusCode(500, new ErrorDto("Error to get transactions."));
        }
    }

    private async Task<List<Credit>> GetCreditsFromAccount(Account account)
    {
        var credits = await _transactionsRepository.GetCreditsFromAccount(account.GetFields().AccountNumber);
        return credits;
    }

    private async Task<List<Debit>> GetDebitsFromAccount(Account account)
    {
        var debits = await _transactionsRepository.GetDebitsFromAccount(account.GetFields().AccountNumber);
        return debits;
    }

    private async Task<List<Transfer>> GetTransfersFromAccount(Account account)
    {
        var transfers = await _transactionsRepository.GetTransfersFromAccount(account.GetFields().AccountNumber);
        return transfers;
    }

    private ArrayList SortTransactionsByDateTime(
        List<Credit> credits, List<Debit> debits, List<Transfer> transfers, Account account)
    {
        var sortedTransactions = new ArrayList();
        int creditIndex = 0, debitIndex = 0, transferIndex = 0;
        int max = credits.Count() + debits.Count() + transfers.Count();
        List<long> creditTimestamps = credits
            .Select(c => new DateTimeOffset(c.GetFields().CreatedAt).ToUnixTimeMilliseconds()).ToList();
        List<long> debitTimestamps = debits
            .Select(d => new DateTimeOffset(d.GetFields().CreatedAt).ToUnixTimeMilliseconds()).ToList();
        List<long> transferTimestamps = transfers
            .Select(t => new DateTimeOffset(t.GetFields().CreatedAt).ToUnixTimeMilliseconds()).ToList();
        creditTimestamps.Add(long.MaxValue);
        debitTimestamps.Add(long.MaxValue);
        transferTimestamps.Add(long.MaxValue);
        for (int index = 0; index < max; index++)
        {
            var creditTimestamp = creditTimestamps.ElementAt(creditIndex);
            var debitTimestamp = debitTimestamps.ElementAt(debitIndex);
            var transferTimestamp = transferTimestamps.ElementAt(transferIndex);
            if (creditTimestamp <= debitTimestamp && creditTimestamp <= transferTimestamp)
            {
                var credit = credits.ElementAt(creditIndex);
                sortedTransactions.Add(new TransactionCreditDebitDto(credit.GetDataWithoutAccount()));
                creditIndex++;
            }
            else if (debitTimestamp <= transferTimestamp)
            {
                var debit = debits.ElementAt(debitIndex);
                sortedTransactions.Add(new TransactionCreditDebitDto(debit.GetDataWithoutAccount()));
                debitIndex++;
            }
            else
            {
                var transfer = transfers.ElementAt(transferIndex);
                sortedTransactions.Add(transfer.GetData(account));
                transferIndex++;
            }
        }
        return sortedTransactions;
    }
}