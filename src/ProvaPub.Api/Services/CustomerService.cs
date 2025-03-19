using Microsoft.EntityFrameworkCore;
using ProvaPub.Contracts.Interfaces;
using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
    public class CustomerService : BaseService<Customer>, ICustomerService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        public CustomerService(TestDbContext ctx, IDateTimeProvider dateTimeProvider) : base(ctx)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<PaginatedResult<Customer>> ListCustomers(int page)
        {
            return await GetPaginatedList(page);
        }

        public async Task<bool> CanPurchase(int customerId, decimal purchaseValue)
        {
            ValidateInputParameters(customerId, purchaseValue);

            //Business Rule: Non registered Customers cannot purchase
            _ = await GetCustomerOrThrow(customerId);

            if (!await HasNotPurchasedThisMonth(customerId))
                return false;

            //Business Rule: A customer can purchase only once per month
            if (!await IsWithinPurchaseLimits(customerId, purchaseValue))
                return false;

            //Business Rule: A customer can purchases only during business hours and working days
            if (!IsWithinBusinessHours())
                return false;

            return true;
        }

        private void ValidateInputParameters(int customerId, decimal purchaseValue)
        {
            if (customerId <= 0) throw new ArgumentOutOfRangeException(nameof(customerId));
            if (purchaseValue <= 0) throw new ArgumentOutOfRangeException(nameof(purchaseValue));
        }

        private async Task<Customer> GetCustomerOrThrow(int customerId)
        {
            var customer = await _ctx.Customers.FindAsync(customerId);
            if (customer == null)
                throw new InvalidOperationException($"Customer Id {customerId} does not exists");

            return customer;
        }

        private async Task<bool> IsWithinPurchaseLimits(int customerId, decimal purchaseValue)
        {
            var haveBoughtBefore = await _ctx.Customers.CountAsync(s => s.Id == customerId && s.Orders.Any());
            return haveBoughtBefore > 0 || purchaseValue <= 100;
        }

        private async Task<bool> HasNotPurchasedThisMonth(int customerId)
        {
            var baseDate = _dateTimeProvider.UtcNow.AddMonths(-1);
            var ordersInThisMonth = await _ctx.Orders.CountAsync(s => s.CustomerId == customerId && s.OrderDate >= baseDate);
            return ordersInThisMonth == 0;
        }

        private bool IsWithinBusinessHours()
        {
            var now = _dateTimeProvider.UtcNow;
            bool isWithinMorningHours = now.Hour > 8 || (now.Hour == 8 && now.Minute >= 0);
            bool isWithinEveningHours = now.Hour < 18 || (now.Hour == 18 && now.Minute == 0);
            bool isWeekday = now.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday);

            return isWithinMorningHours && isWithinEveningHours && isWeekday;
        }
    }
}
