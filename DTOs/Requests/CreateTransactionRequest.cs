using WalletApi.Models;

namespace WalletApi.DTOs.Requests
{
    public class CreateTransactionRequest
    {
        public TransactionType Type {get; set;}
        public decimal Amount {get; set;}
    }
}