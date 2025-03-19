using ProvaPub.Models;

namespace ProvaPub.Contracts.Interfaces
{
    public interface IOrderService
    {
        Task<Order> PayOrder(string paymentMethod, decimal paymentValue, int customerId);
    }
}
