using WalletApi.Models;

namespace WalletApi.DTOs.Responses
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }

        public TransactionType Type { get; set; }

        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}