using Microsoft.EntityFrameworkCore;
using ProvaPub.Contracts.Interfaces;
using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
	public class ProductService : BaseService<Product>, IProductService
	{
		public ProductService(TestDbContext ctx) : base(ctx)
		{
		}

		public async Task<PaginatedResult<Product>> ListProducts(int page)
		{
			return await GetPaginatedList(page);
		}

	}
}
