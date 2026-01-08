using HotelListing.Api.Models.Domain;

namespace HotelListing.Api.Repositories.Interface
{
    public interface IHotelsRepository : IGenericRepository<Hotel>
    {
        Task<Hotel?> GetHotelDetails(int id);
    }
}
