using Microsoft.AspNetCore.Mvc;
using WalletApi.Data;
using WalletApi.Models;
using Microsoft.EntityFrameworkCore;
using WalletApi.DTOs.Requests;

namespace WalletApi.Controllers
{
    [ApiController]
    [Route("transfers")]
    public class TransfersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TransfersController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> CreateTransfer(CreateTransferRequest request,
        [FromHeader(Name= "Idempotency-Key")] string idempotencyKey)
        {

            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return BadRequest("O Header Idempotency-Key é obrigatório.");
            }


            await using var transaction =
                await _context.Database.BeginTransactionAsync();


            try
            {
                // Verifica se requisição já foi processada
                var existing = await _context.IdempotencyRecords
                    .FirstOrDefaultAsync(x => x.Key == idempotencyKey);

                if (existing != null)
                {
                    return Ok(new
                    {
                        transferId = existing.TransferId
                    });
                }

                var fromAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == request.FromAccountId);

                var toAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == request.ToAccountId);

                if (fromAccount == null || toAccount == null)
                    return NotFound("Conta não encontrada.");

                if (request.FromAccountId == request.ToAccountId)
                    return BadRequest("A conta de origem e destino devem ser diferentes.");

                // Cria lançamentos
                var transferOut = fromAccount.TransferOut(request.Amount);
                var transferIn = toAccount.TransferIn(request.Amount);

                _context.Transactions.Add(transferOut);
                _context.Transactions.Add(transferIn);


                // Cria a transferência e relaciona as duas transações
                var transfer = new Transfer
                {
                    Id = Guid.NewGuid(),
                    FromAccountId = fromAccount.Id,
                    ToAccountId = toAccount.Id,
                    Amount = request.Amount,
                    OutTransactionId = transferOut.Id,
                    InTransactionId = transferIn.Id
                };

                _context.Transfers.Add(transfer);


                // Salva chave de idempotência
                var idempotency = new IdempotencyRecord(
                    idempotencyKey,
                    transfer.Id
                );

                _context.IdempotencyRecords.Add(idempotency);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    transferId = transfer.Id,
                    fromAccount = fromAccount.Id,
                    toAccount = toAccount.Id,
                    amount = request.Amount
                });

            }
            catch(DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return Conflict(
                    "Uma das contas foi alterada por outra operação. Tente novamente."
                );
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
