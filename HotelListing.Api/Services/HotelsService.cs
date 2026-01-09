using AutoMapper;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Hotel;
using HotelListing.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services
{
    public class HotelsService(IHotelsRepository repository, IMapper mapper) : IHotelsService
    {
        public async Task<IEnumerable<GetHotelsDto>> GetHotelsAsync()
        {
            var hotels = await repository.GetAllAsync();

            var response = mapper.Map<List<GetHotelsDto>>(hotels);

            return response;
        }

        public async Task<GetHotelDto?> GetHotelAsync(int id)
        {
            var hotel = await repository.GetHotelDetails(id);

            if (hotel == null)
            {
                return null;
            }

            var response = mapper.Map<GetHotelDto>(hotel);

            return response;
        }

        public async Task<GetHotelDto> CreateHotelAsync(CreateHotelDto hotelDto)
        {
            var hotel = mapper.Map<Hotel>(hotelDto);

            await repository.AddAsync(hotel);

            var fullHotel = await repository.GetHotelDetails(hotel.Id);

            var createdHotelDto = mapper.Map<GetHotelDto>(fullHotel);

            return createdHotelDto;
        }

        public async Task<bool> UpdateHotelAsync(int id, UpdateHotelDto hotelDto)
        {
            var hotel = await repository.GetByIdAsync(id);

            if (hotel == null)
            {
                return false;
            }

            mapper.Map(hotelDto, hotel);

            try
            {
                await repository.UpdateAsync(hotel);
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

        public async Task<bool> DeleteHotelAsync(int id)
        {
            var hotel = await repository.GetByIdAsync(id);

            if (hotel == null)
            {
                return false;
            }

            await repository.DeleteAsync(id);

            return true;
        }      
    }
}
