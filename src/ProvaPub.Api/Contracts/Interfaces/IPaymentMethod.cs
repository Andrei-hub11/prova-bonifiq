namespace ProvaPub.Contracts.Interfaces
{
    public interface IPaymentMethod
    {
        Task<bool> ProcessPayment(decimal amount);
    }
}
