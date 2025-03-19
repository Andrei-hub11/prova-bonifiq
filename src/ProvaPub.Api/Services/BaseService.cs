using Microsoft.EntityFrameworkCore;
using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
    public abstract class BaseService<T> where T : class
    {
        protected readonly TestDbContext _ctx;

        protected BaseService(TestDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<PaginatedResult<T>> GetPaginatedList(int page, int pageSize = 4)
        {
            var query = _ctx.Set<T>();

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var totalCount = await query.CountAsync();

            return new PaginatedResult<T>(items, totalCount, page * pageSize < totalCount);
        }
    }

}
