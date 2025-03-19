using ProvaPub.Contracts.Interfaces;

namespace ProvaPub.Services
{
    public class PixPaymentMethod : IPaymentMethod
    {
        public Task<bool> ProcessPayment(decimal amount)
        {
            // Processa o pagamento
            return Task.FromResult(true);
        }
    }
}
