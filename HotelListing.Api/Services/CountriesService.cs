using AutoMapper;
using HotelListing.Api.Models.DTOs.Country;
using HotelListing.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services
{
    public class CountriesService(ICountriesRepository repository, IMapper mapper) : ICountriesService
    {
        public async Task<IEnumerable<GetCountriesDto>> GetCountriesAsync()
        {
            var countries = await repository.GetAllAsync();

            var response = mapper.Map<List<GetCountriesDto>>(countries);

            return response;
        }

        public async Task<GetCountryDto?> GetCountryAsync(int id)
        {
            var country = await repository.GetCountryDetails(id);

            if (country == null)
            {
                return null;
            }

            var response = mapper.Map<GetCountryDto>(country);

            return response;
        }

        public async Task<GetCountryDto> CreateCountryAsync(CreateCountryDto countryDto)
        {
            var country = mapper.Map<Country>(countryDto);

            await repository.AddAsync(country);

            var createdCountryDto = mapper.Map<GetCountryDto>(country);

            return createdCountryDto;
        }

        public async Task<bool> UpdateCountryAsync(int id, UpdateCountryDto countryDto)
        {
            var country = await repository.GetByIdAsync(id);

            if (country == null)
            {
                return false;
            }

            mapper.Map(countryDto, country);

            try
            {
                await repository.UpdateAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await repository.Exists(id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }

            return true;
        }

        public async Task<bool> DeleteCountryAsync(int id)
        {
            var country = await repository.GetByIdAsync(id);

            if (country == null)
            {
                return false;
            }

            await repository.DeleteAsync(id);

            return true;
        }
    }
}
