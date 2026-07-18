using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WalletApi.Models
{
    public class Transaction
    {
    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public Account Account { get; private set; } = default!;

    public TransactionType Type { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime CreatedAt { get; private set; }
    }
}