namespace WalletApi.Models
{
    public class IdempotencyRecord
    {
        public Guid Id { get; private set; }

        public string Key { get; private set; } = null!;

        public Guid? TransactionId {get; private set;}

        public Guid? TransferId { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private IdempotencyRecord() { }

        public IdempotencyRecord(
            string key,
            Guid? transactionId = null,
            Guid? transferId = null)
        {
            Id = Guid.NewGuid();
            Key = key;
            TransactionId = transactionId;
            TransferId = transferId;
            CreatedAt = DateTime.UtcNow;
        }
    }
}


