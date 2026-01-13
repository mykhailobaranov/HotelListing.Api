using AutoMapper;
using HotelListing.Api.Models;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Hotel;
using HotelListing.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services
{
    public class HotelsService(
        IHotelsRepository hotelsRepository,
        ICountriesRepository countriesRepository,
        IMapper mapper) : IHotelsService
    {
        public async Task<Result<IEnumerable<GetHotelsDto>>> GetHotelsAsync()
        {
            var hotels = await hotelsRepository.GetAllAsync();

            var response = mapper.Map<IEnumerable<GetHotelsDto>>(hotels);

            return Result<IEnumerable<GetHotelsDto>>.Success(response);
        }   

        public async Task<Result<GetHotelDto>> GetHotelAsync(int id)
        {
            var hotel = await hotelsRepository.GetHotelDetails(id);

            if (hotel == null)
            {
                return Result<GetHotelDto>.NotFound();
            }

            var response = mapper.Map<GetHotelDto>(hotel);

            return Result<GetHotelDto>.Success(response);
        }

        public async Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto)
        {
            var isExist = await hotelsRepository.
                ExistsAsync(h => h.Name == hotelDto.Name &&
                            h.CountryId == hotelDto.CountryId);

            if (isExist)
            {
                return Result<GetHotelDto>.Conflict($"Hotel with name '{hotelDto.Name}' already exists in this country.");
            }

            var country = await countriesRepository.GetByIdAsync(hotelDto.CountryId);

            if(country == null)
            {
                return Result<GetHotelDto>.NotFound($"Country with id {hotelDto.CountryId} does not exist.");
            }

            var hotel = mapper.Map<Hotel>(hotelDto);
            await hotelsRepository.AddAsync(hotel);
            var fullHotel = await hotelsRepository.GetHotelDetails(hotel.Id);
            var createdHotelDto = mapper.Map<GetHotelDto>(fullHotel);

            return Result<GetHotelDto>.Success(createdHotelDto);
        }

        public async Task<Result> UpdateHotelAsync(int id, UpdateHotelDto hotelDto)
        {
            if (id != hotelDto.Id)
            {
                return Result.BadRequest($"Route Id: {id} does not match Body Id: {hotelDto.Id}.");
            }

            var hotel = await hotelsRepository.GetByIdAsync(id);

            if (hotel == null)
            {
                return Result.NotFound();
            }

            mapper.Map(hotelDto, hotel);

            try
            {
                await hotelsRepository.UpdateAsync(hotel);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await hotelsRepository.Exists(id))
                {
                    return Result.Conflict($"Hotel with id {id} no longer exists.");
                }
                else
                {
                    throw;
                }
            }

            return Result.Success();
        }

        public async Task<Result> DeleteHotelAsync(int id)
        {
            var hotel = await hotelsRepository.GetByIdAsync(id);

            if (hotel == null)
            {
                return Result.NotFound();
            }

            await hotelsRepository.DeleteAsync(id);

            return Result.Success();
        }      
    }
}
