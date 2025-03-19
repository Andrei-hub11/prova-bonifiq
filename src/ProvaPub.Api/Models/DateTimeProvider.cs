using ProvaPub.Contracts.Interfaces;

namespace ProvaPub.Models
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
