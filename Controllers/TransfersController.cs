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
        public async Task<IActionResult> CreateTransfer(CreateTransferRequest request)
        {
            var fromAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.FromAccountId);
            
            var toAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == request.ToAccountId);

            if (fromAccount == null || toAccount == null)
                return NotFound("Conta não encontrada.");

            if (fromAccount == toAccount)
                return BadRequest("A conta de origem e de destino devem ser diferentes.");

            // await using var transaction =
            //     await _context.Database.BeginTransactionAsync();

            Console.WriteLine($"FromAccount: {fromAccount?.Id}");
            Console.WriteLine($"ToAccount: {toAccount?.Id}");
            
            // garante que as duas operações aconteçam
            try
            {
                var transferOut = fromAccount.TransferOut(request.Amount);

                var transferIn = toAccount.TransferIn(request.Amount);

                _context.Transactions.Add(transferOut);
                _context.Transactions.Add(transferIn);

                await _context.SaveChangesAsync();
                // await transaction.CommitAsync();

                return Ok(new
                {
                    fromAccount = fromAccount.Id,
                    toAccount = toAccount.Id,
                    amount = request.Amount
                });
    
            }
            catch
            {
                // await transaction.RollbackAsync();
                throw;
            }

        }
    }
}
