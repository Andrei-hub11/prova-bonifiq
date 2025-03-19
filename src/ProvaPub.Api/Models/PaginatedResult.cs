namespace ProvaPub.Models
{
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public bool HasNext { get; set; }

        public PaginatedResult(List<T> items, int totalCount, bool hasNext)
        {
            Items = items;
            TotalCount = totalCount;
            HasNext = hasNext;
        }
    }
}
