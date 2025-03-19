using Microsoft.EntityFrameworkCore;
using ProvaPub.Contracts.Interfaces;
using ProvaPub.Models;
using ProvaPub.Repository;
using ProvaPub.Services;
using Moq;

namespace ProvaPub.Tests
{
    public class CustomerServiceTests
    {

        private (ICustomerService, Mock<IDateTimeProvider>, TestDbContext) GetCustomerService()
        {
            // Ensure a new database is created for each test
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var fakeDbContext = new TestDbContext(options);

            // Setup mock DateTimeProvider
            var mockDateTimeProvider = new Mock<IDateTimeProvider>();

            // Create the service with dependencies
            var customerService = new CustomerService(fakeDbContext, mockDateTimeProvider.Object);

            return (customerService, mockDateTimeProvider, fakeDbContext);
        }

        [Fact]
        public async Task CanPurchase_CustomerNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var customerId = 1;
            var purchaseValue = 100m;

            var (customerService, fakeDbContext, _) = GetCustomerService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await customerService.CanPurchase(customerId, purchaseValue));

            // Assert that the exception message is correct
            Assert.Equal($"Customer Id {customerId} does not exists", exception.Message);
        }

        [Theory]
        [InlineData(0, 50)]  // customerId inv�lido
        [InlineData(-1, 50)] // customerId inv�lido
        [InlineData(1, 0)]   // purchaseValue inv�lido
        [InlineData(1, -10)] // purchaseValue inv�lido
        public async Task CanPurchase_InvalidParameters_ThrowsArgumentOutOfRangeException(int customerId, decimal purchaseValue)
        {
            // Arrange
            var (customerService, _, _) = GetCustomerService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await customerService.CanPurchase(customerId, purchaseValue));
        }

        [Theory]
        [InlineData(8, 0, true)]  // 08:00 must be allowed
        [InlineData(18, 0, true)] // 18:00 must be allowed
        [InlineData(7, 59, false)] // 07:59 must be denied
        [InlineData(18, 1, false)] // 18:01 must be denied
        public async Task CanPurchase_BorderBusinessHours_ReturnsExpectedResult(int hour, int minute, bool expectedResult)
        {
            // Arrange
            var customerId = 1;
            var purchaseValue = 50m;
            var currentDate = new DateTime(2023, 10, 16, hour, minute, 0); // Monday

            var (customerService, mockDateTimeProvider, fakeDbContext) = GetCustomerService();

            var customer = new Customer { Id = customerId, Name = "Test Customer" };
            fakeDbContext.Customers.Add(customer);
            await fakeDbContext.SaveChangesAsync();

            mockDateTimeProvider.Setup(p => p.UtcNow).Returns(currentDate);

            // Act
            var result = await customerService.CanPurchase(customerId, purchaseValue);

            // Assert
            Assert.Equal(expectedResult, result);
        }


        [Fact]
        public async Task CanPurchase_CustomerAlreadyPurchasedThisMonth_ReturnsFalse()
        {
            // Arrange
            var customerId = 1;
            var purchaseValue = 100m;
            var currentDate = new DateTime(2023, 10, 15, 10, 0, 0); // A weekday at 10:00 AM

            var (customerService, mockDateTimeProvider, fakeDbContext) = GetCustomerService();

            // Add customer
            var customer = new Customer { Id = customerId, Name = "Test Customer" };
            fakeDbContext.Customers.Add(customer);

            // Add order in the current month
            var order = new Order
            {
                Id = 1,
                CustomerId = customerId,
                OrderDate = currentDate.AddDays(-5), // 5 days ago, but same month
                Value = 50m
            };

            fakeDbContext.Orders.Add(order);

            await fakeDbContext.SaveChangesAsync();

            // Setup current date
            mockDateTimeProvider.Setup(p => p.UtcNow).Returns(currentDate);

            // Act
            var result = await customerService.CanPurchase(customerId, purchaseValue);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanPurchase_PurchaseValueExceedsLimit_ReturnsFalse()
        {
            // Arrange
            var customerId = 1;
            var purchaseValue = 101m; // Assuming limit is 100
            var currentDate = new DateTime(2023, 10, 15, 10, 0, 0); // A weekday at 10:00 AM

            var (customerService, mockDateTimeProvider, fakeDbContext) = GetCustomerService();

            // Add customer
            var customer = new Customer { Id = customerId, Name = "Test Customer" };
            fakeDbContext.Customers.Add(customer);

            // Add order from previous month
            var order = new Order
            {
                Id = 1,
                CustomerId = customerId,
                OrderDate = new DateTime(2023, 9, 15), // Previous month
                Value = 50m
            };

            fakeDbContext.Orders.Add(order);
            await fakeDbContext.SaveChangesAsync();

            // Setup current date
            mockDateTimeProvider.Setup(p => p.UtcNow).Returns(currentDate);

            // Act
            var result = await customerService.CanPurchase(customerId, purchaseValue);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanPurchase_OutsideBusinessHours_ReturnsFalse()
        {
            // Arrange
            var customerId = 1;
            var purchaseValue = 50m;
            var currentDate = new DateTime(2023, 10, 15, 19, 0, 0); // A weekday at 7:00 PM (outside 8:00-18:00)

            var (customerService, mockDateTimeProvider, fakeDbContext) = GetCustomerService();

            // Add customer
            var customer = new Customer { Id = customerId, Name = "Test Customer" };
            fakeDbContext.Customers.Add(customer);
            await fakeDbContext.SaveChangesAsync();

            // Setup current date
            mockDateTimeProvider.Setup(p => p.UtcNow).Returns(currentDate);

            // Act
            var result = await customerService.CanPurchase(customerId, purchaseValue);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanPurchase_OnWeekend_ReturnsFalse()
        {
            // Arrange
            var customerId = 1;
            var purchaseValue = 50m;
            var currentDate = new DateTime(2023, 10, 14, 10, 0, 0); // Saturday at 10:00 AM

            var (customerService, mockDateTimeProvider, fakeDbContext) = GetCustomerService();

            // Add customer
            var customer = new Customer { Id = customerId, Name = "Test Customer" };
            fakeDbContext.Customers.Add(customer);
            await fakeDbContext.SaveChangesAsync();

            // Setup current date
            mockDateTimeProvider.Setup(p => p.UtcNow).Returns(currentDate);

            // Act
            var result = await customerService.CanPurchase(customerId, purchaseValue);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanPurchase_AllConditionsMet_ReturnsTrue()
        {
            // Arrange
            var customerId = 1;
            var purchaseValue = 50m;
            var currentDate = new DateTime(2023, 10, 16, 10, 0, 0); // Monday at 10:00 AM

            var (customerService, mockDateTimeProvider, fakeDbContext) = GetCustomerService();

            // Add customer
            var customer = new Customer { Id = customerId, Name = "Test Customer" };
            fakeDbContext.Customers.Add(customer);

            // Add order from previous month
            var order = new Order
            {
                Id = 1,
                CustomerId = customerId,
                OrderDate = new DateTime(2023, 9, 15), // Previous month
                Value = 50m
            };

            fakeDbContext.Orders.Add(order);
            await fakeDbContext.SaveChangesAsync();

            // Setup current date
            mockDateTimeProvider.Setup(p => p.UtcNow).Returns(currentDate);

            // Act
            var result = await customerService.CanPurchase(customerId, purchaseValue);

            // Assert
            Assert.True(result);
        }
    }
}
