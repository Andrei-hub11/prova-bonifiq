using ProvaPub.Models;
using ProvaPub.Repository;
using ProvaPub.Contracts.Interfaces;

namespace ProvaPub.Services
{
	public class OrderService : IOrderService
	{
		private readonly TestDbContext _ctx;
		private readonly IPaymentMethodFactory _paymentMethodFactory;
		public OrderService(TestDbContext ctx, IPaymentMethodFactory paymentMethodFactory)
		{
			_ctx = ctx;
			_paymentMethodFactory = paymentMethodFactory;
		}

		public async Task<Order> PayOrder(string paymentMethod, decimal paymentValue, int customerId)
		{
			var payMethod = _paymentMethodFactory.GetPaymentMethod(paymentMethod);

			await payMethod.ProcessPayment(paymentValue);

			return await InsertOrder(new Order()
			{
				Value = paymentValue,
				CustomerId = customerId,
				OrderDate = DateTime.UtcNow
			});
		}

		private async Task<Order> InsertOrder(Order order)
		{
			var entity = (await _ctx.Orders.AddAsync(order)).Entity;
			await _ctx.SaveChangesAsync();
			return entity;
		}
	}
}
