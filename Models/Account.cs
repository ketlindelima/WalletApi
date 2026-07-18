using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WalletApi.Models
{
    public class Account
    {
        public Guid Id {get; private set;}
        public decimal Balance {get; private set;}
        public DateTime CreatedAt {get; private set;}
        public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();

        public void Credit(decimal amount)
        {
            Balance += amount;
        }

        public void Debit(decimal amount)
        {
            if (Balance < amount)
                throw new InsufficientBalanceException();
            
            Balance -= amount;
        }
    }
}