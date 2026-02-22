using HotelListing.Api.Data;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Repositories.Implementation;

public class CountriesRepository : GenericRepository<Country>, ICountriesRepository
{
    private readonly HotelListingDbContext _db;

    public CountriesRepository(HotelListingDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<Country?> GetCountryWithHotelsAsync(int id)
    {
        return await _db.Countries
            .Include(q => q.Hotels)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.CountryId == id);
    }
}   