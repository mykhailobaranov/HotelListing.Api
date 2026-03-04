using HotelListing.Api.Models.Domain;

namespace HotelListing.Api.Repositories.Interface;

public interface IHotelsRepository : IGenericRepository<Hotel>
{
    Task<Hotel?> GetHotelWithCountryAsync(int id);
    Task<Hotel?> GetHotelWithCountryAndAdminsAsync(int id);
}