using Microsoft.EntityFrameworkCore;
namespace WebProveedoresN.Models
{
    public class Pagination<T> : List<T>
    {
        public int PageNumber { get; private set; }
        public int TotalPages { get; private set; }
        public Pagination(List<T> items, int count, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public static Pagination<T> CreatePagination(List<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new Pagination<T>(items, count, pageNumber, pageSize);
        }
    }
}
