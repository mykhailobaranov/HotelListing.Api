using HotelListing.Api.Data;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Repositories.Interface;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Repositories.Implementation
{
    public class HotelsRepository : GenericRepository<Hotel>, IHotelsRepository
    {
        private readonly HotelListingDbContext _db;

        public HotelsRepository(HotelListingDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Hotel?> GetHotelDetails(int id)
        {
            return await _db.Hotels
                .Include(q => q.Country)
                .FirstOrDefaultAsync(q => q.Id == id);
        }
    }
}
