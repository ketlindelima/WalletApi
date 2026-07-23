using System;
using WalletApi.Migrations;

namespace WalletApi.Models
{
    public class Account
    {
        public Guid Id {get; private set;}
        public decimal Balance {get; private set;}
        public DateTime CreatedAt {get; private set;}
        private readonly List<Transaction> _transactions = [];
        public IReadOnlyCollection<Transaction> Transactions => _transactions;
        public Account()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        public Transaction Credit(decimal amount)
        {
            Balance += amount;

            var transaction = new Transaction(
                Id,
                TransactionType.Credit,
                amount
            );

            _transactions.Add(transaction);

            return transaction;
        }


        public Transaction Debit(decimal amount)
        {
            if (Balance < amount)
                throw new InvalidOperationException("Saldo insuficiente.");

            Balance -= amount;

            var transaction = new Transaction(
                Id,
                TransactionType.Debit,
                amount
            );

            _transactions.Add(transaction);

            return transaction;
        }

        public Transaction TransferIn(decimal amount)
        {           
            Balance += amount;

            var transaction = new Transaction(
                Id,
                TransactionType.TransferIn,
                amount
            );
            _transactions.Add(transaction);
            return transaction;
        }

        public Transaction TransferOut(decimal amount)
        {
            if (Balance < amount)
                throw new InvalidOperationException("Seu saldo é insuficiente.");
            
            Balance -= amount;

            var transaction = new Transaction(
                Id,
                TransactionType.TransferOut,
                amount
            );
            _transactions.Add(transaction);
            return transaction;
        }
    }
}