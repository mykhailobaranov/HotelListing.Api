using AutoMapper;
using HotelListing.Api.Models;
using HotelListing.Api.Models.DTOs.Country;
using HotelListing.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services
{
    public class CountriesService(ICountriesRepository repository, IMapper mapper) : ICountriesService
    {
        public async Task<Result<IEnumerable<GetCountriesDto>>> GetCountriesAsync()
        {
            var countries = await repository.GetAllAsync();

            var response = mapper.Map<List<GetCountriesDto>>(countries);

            return Result<IEnumerable<GetCountriesDto>>.Success(response);
        }

        public async Task<Result<GetCountryDto>> GetCountryAsync(int id)
        {
            var country = await repository.GetCountryDetails(id);

            if (country == null)
            {
                return Result<GetCountryDto>.NotFound();
            }

            var response = mapper.Map<GetCountryDto>(country);

            return Result<GetCountryDto>.Success(response);
        }

        public async Task<Result<GetCountryDto>> CreateCountryAsync(CreateCountryDto countryDto)
        {
            var isExist = await repository.ExistsAsync(c => c.Name == countryDto.Name);

            if (isExist)
            {
                return Result<GetCountryDto>.Conflict($"Country with name '{countryDto.Name}' already exists.");
            }

            var country = mapper.Map<Country>(countryDto);

            await repository.AddAsync(country);

            var createdCountryDto = mapper.Map<GetCountryDto>(country);

            return Result<GetCountryDto>.Success(createdCountryDto);
        }

        public async Task<Result> UpdateCountryAsync(int id, UpdateCountryDto countryDto)
        {
            if (id != countryDto.CountryId)
            {
                return Result.BadRequest($"Route Id: {id} does not match Body Id: {countryDto.CountryId}.");
            }

            var country = await repository.GetByIdAsync(id);

            if (country == null)
            {
                return Result.NotFound($"Country with id {id} no longer exists.");
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
                    return Result.Conflict($"Country with id {id} no longer exists.");
                }
                else
                {
                    throw;
                }
            }

            return Result.Success();
        }

        public async Task<Result> DeleteCountryAsync(int id)
        {
            var country = await repository.GetByIdAsync(id);

            if (country == null)
            {
                return Result.NotFound();
            }

            await repository.DeleteAsync(id);

            return Result.Success();
        }
    }
}
