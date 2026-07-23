using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WalletApi.DTOs.Requests
{
    public class CreateTransferRequest
    {
        public Guid FromAccountId {get; set;}
        public Guid ToAccountId {get; set;}
        public decimal Amount {get; set;}
    }
}