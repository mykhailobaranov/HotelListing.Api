using HotelListing.Api.Data;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Repositories.Implementation
{
    public class CountriesRepository(HotelListingDbContext db) : ICountriesRepository
    {
        public async Task AddAsync(Country entity)
        {
            await db.Countries.AddAsync(entity);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var country = await db.Countries.FindAsync(id);
            db.Countries.Remove(country!);
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Country>> GetAllAsync()
        {
            return await db.Countries.ToListAsync();
        }

        public async Task<Country?> GetByIdAsync(int id)
        {
            return await db.Countries.FindAsync(id);
        }

        public async Task UpdateAsync(Country entity)
        {
            db.Countries.Update(entity);
            await db.SaveChangesAsync();
        }

        public async Task<bool> Exists(int id)
        {
            return await db.Countries.FindAsync(id) != null;
        }

        public async Task<Country?> GetCountryDetails(int id)
        {
            return await db.Countries
                .Include(q => q.Hotels)
                .FirstOrDefaultAsync(q => q.CountryId == id);
        }
    }
}
