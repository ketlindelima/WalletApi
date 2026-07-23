using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WalletApi.Models
{
    public class Transfer
    {
        public Guid Id { get; set; }

        public Guid FromAccountId { get; set; }

        public Guid ToAccountId { get; set; }

        public decimal Amount { get; set; }

        public Guid OutTransactionId { get; set; }

        public Guid InTransactionId { get; set; }
    }

}