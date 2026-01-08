using HotelListing.Api.Data;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Repositories.Interface;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Repositories.Implementation
{
    public class HotelsRepository(HotelListingDbContext db) : IHotelsRepository
    {
        public async Task AddAsync(Hotel entity)
        {
            await db.Hotels.AddAsync(entity);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var hotel = await db.Hotels.FindAsync(id);
            db.Hotels.Remove(hotel!);
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Hotel>> GetAllAsync()
        {
            return await db.Hotels.ToListAsync();
        }

        public async Task<Hotel?> GetByIdAsync(int id)
        {
            return await db.Hotels.FindAsync(id);
        }

        public async Task UpdateAsync(Hotel entity)
        {
            db.Hotels.Update(entity);
            await db.SaveChangesAsync();
        }
        public async Task<bool> Exists(int id)
        {
            return await db.Hotels.FindAsync(id) != null;
        }

        public async Task<Hotel?> GetHotelDetails(int id)
        {
            return await db.Hotels
                .Include(q => q.Country)
                .FirstOrDefaultAsync(q => q.Id == id);
        }
    }
}
