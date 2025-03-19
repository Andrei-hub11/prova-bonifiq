using ProvaPub.Models;

namespace ProvaPub.Contracts.Interfaces
{
    public interface IProductService
    {
        Task<PaginatedResult<Product>> ListProducts(int page);
    }
}
