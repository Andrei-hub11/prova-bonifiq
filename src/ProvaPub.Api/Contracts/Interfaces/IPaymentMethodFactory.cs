namespace ProvaPub.Contracts.Interfaces
{
    public interface IPaymentMethodFactory
    {
        IPaymentMethod GetPaymentMethod(string paymentMethod);
    }
}
