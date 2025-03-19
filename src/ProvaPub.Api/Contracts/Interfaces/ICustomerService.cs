using ProvaPub.Models;

namespace ProvaPub.Contracts.Interfaces
{
    public interface ICustomerService
    {
        Task<PaginatedResult<Customer>> ListCustomers(int page);
        Task<bool> CanPurchase(int customerId, decimal purchaseValue);
    }
}
