namespace WalletApi.DTOs.Requests
{
    public class TransactionQueryRequest
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}