using HotelListing.Api.Models.Domain;

namespace HotelListing.Api.Repositories.Interface;

public interface ICountriesRepository : IGenericRepository<Country>
{
    Task<Country?> GetCountryWithHotelsAsync(int id);
}