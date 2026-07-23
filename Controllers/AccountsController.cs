using Microsoft.AspNetCore.Mvc;
using WalletApi.Data;
using WalletApi.Models;
using Microsoft.EntityFrameworkCore;
using WalletApi.DTOs.Requests;
using WalletApi.DTOs.Responses;

namespace WalletApi.Controllers
{   
     
    [ApiController]
    [Route("accounts")]
    public class AccountsController : Controller
    {
        private readonly AppDbContext _context;
        public AccountsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> CreateAccount()
        {
            var account = new Account();
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created, account);
        }


        [HttpGet("{id:guid}/balance")]
        public async Task<IActionResult> GetBalance(Guid id)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id);

            if (account == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                balance = account.Balance
            });
        }

        [HttpPost("{id:guid}/transactions")]
        public async Task<IActionResult> CreateTransaction(
        Guid id,
        CreateTransactionRequest request,
        [FromHeader(Name= "Idempotency-Key")] string idempotencyKey)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == id);

            if (account == null)
            {
                return NotFound();
            }

            var existing = await _context.IdempotencyRecords
                .FirstOrDefaultAsync(x => x.Key == idempotencyKey);

            if (existing != null)
            {
                return Ok(new
                {
                    transactionId = existing.TransactionId
                });
            }

            Transaction createdTransaction;

            switch(request.Type)
            {
                case TransactionType.Credit:
                    createdTransaction = account.Credit(request.Amount);
                    _context.Transactions.Add(createdTransaction);
                    break;

                case TransactionType.Debit:
                    createdTransaction = account.Debit(request.Amount);
                    _context.Transactions.Add(createdTransaction);
                    break;

                case TransactionType.TransferIn:
                case TransactionType.TransferOut:
                    return BadRequest("Transferências devem ser realizadas pelo endpoint de transferência.");

                default:
                    return BadRequest("Tipo de transação inválido.");
            }

            _context.IdempotencyRecords.Add(
                new IdempotencyRecord(
                    idempotencyKey,
                    createdTransaction.Id
                )
            );

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) // Utiliza RowVersion
            {
                return Conflict(
                    "A conta foi alterada por outra operação. Tente novamente."
                );
            }


            return Ok(new
            {
                account.Id,
                account.Balance
            });
        }

        [HttpGet("{id:guid}/transactions")]
        public async Task<IActionResult> GetTransactions(Guid id,
        [FromQuery] TransactionQueryRequest request)
        {
            var account = await _context.Accounts
                .AnyAsync(a => a.Id == id);

            if (!account)
                return NotFound();

            //Evita erros e limita a paginação
            if (request.Page < 1)
                request.Page = 1;

            if (request.PageSize < 1)
                request.PageSize = 10;

            if (request.PageSize > 100)
                request.PageSize = 100;

            var totalItems = await _context.Transactions
    .           CountAsync(t => t.AccountId == id);
    
            //Limita a consulta diretamente no banco
            var transactions = await _context.Transactions
                .Where(t => t.AccountId == id)
                .OrderByDescending (t => t.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new TransactionResponse
                {
                    Id = t.Id,
                    Type = t.Type,
                    Amount = t.Amount,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(
                new PagedResponse<TransactionResponse>(
                    transactions,
                    request.Page,
                    request.PageSize,
                    totalItems
                )
            );
        }
    }
}