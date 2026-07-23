namespace WalletApi.DTOs.Responses
{
    public class PagedResponse<T>
    {
        public IReadOnlyCollection<T> Items { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }


        public PagedResponse(
            IReadOnlyCollection<T> items,
            int page,
            int pageSize,
            int totalItems)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalItems = totalItems;
            TotalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize
            );
        }
    }
}