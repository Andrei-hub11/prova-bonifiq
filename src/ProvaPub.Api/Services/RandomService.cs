using Microsoft.EntityFrameworkCore;
using ProvaPub.Contracts.Interfaces;
using ProvaPub.Models;
using ProvaPub.Repository;

namespace ProvaPub.Services
{
    public class RandomService : IRandomService
    {
        private readonly TestDbContext _ctx;

        public RandomService(TestDbContext ctx)
        {
            _ctx = ctx;
        }
        public async Task<int> GetRandom()
        {
            int maxAttempts = 100;
            int number, attempts = 0;

            do
            {
                if (attempts++ >= maxAttempts)
                {
                    throw new Exception("Não foi possível gerar um número aleatório único. Todos os números possíveis já foram gerados.");
                }

                number = Random.Shared.Next(100);
            } while (await _ctx.Numbers.AnyAsync(n => n.Number == number));

            _ctx.Numbers.Add(new RandomNumber() { Number = number });
            await _ctx.SaveChangesAsync();
            return number;
        }

    }
}
