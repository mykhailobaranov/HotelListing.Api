using HotelListing.Api.Data;
using HotelListing.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Repositories.Implementation
{
    public class GenericRepository<T>(HotelListingDbContext db) : IGenericRepository<T> where T : class
    {
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await db.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await db.Set<T>().FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await db.AddAsync(entity);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            db.Update(entity);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            db.Set<T>().Remove(entity!);
            await db.SaveChangesAsync();
        }

        public async Task<bool> Exists(int id)
        {
            return await GetByIdAsync(id) != null;
        }
    }
}
