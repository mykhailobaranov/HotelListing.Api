using AutoMapper;
using HotelListing.Api.Models;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Hotel;
using HotelListing.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

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
        var hotel = await hotelsRepository.GetHotelWithCountryAsync(id);

        if (hotel == null)
        {
            return Result<GetHotelDto>.NotFound($"Hotel with id {id} was not found.");
        }

        var response = mapper.Map<GetHotelDto>(hotel);

        return Result<GetHotelDto>.Success(response);
    }

    public async Task<Result<GetHotelDto>> CreateHotelAsync(CreateHotelDto hotelDto)
    {
        var isExist = await hotelsRepository
            .ExistsAsync(h => h.Name == hotelDto.Name &&
                        h.CountryId == hotelDto.CountryId);

        if (isExist)
        {
            return Result<GetHotelDto>.Conflict($"Hotel with name '{hotelDto.Name}' already exists in this country.");
        }

        var country = await countriesRepository.GetByIdAsync(hotelDto.CountryId);

        if (country == null)
        {
            return Result<GetHotelDto>.NotFound($"Country with id {hotelDto.CountryId} doesn't exist.");
        }

        var hotel = mapper.Map<Hotel>(hotelDto);
        await hotelsRepository.AddAsync(hotel);

        var fullHotel = await hotelsRepository.GetHotelWithCountryAsync(hotel.Id);
        var response = mapper.Map<GetHotelDto>(fullHotel);

        return Result<GetHotelDto>.Success(response);
    }

    public async Task<Result> UpdateHotelAsync(int id, UpdateHotelDto hotelDto)
    {
        if (id != hotelDto.Id)
        {
            return Result.BadRequest($"Route Id: {id} doesn't match Body Id: {hotelDto.Id}.");
        }

        var hotel = await hotelsRepository.GetByIdAsync(id);

        if (hotel == null)
        {
            return Result.NotFound($"Hotel with id {id} was not found.");
        }

        mapper.Map(hotelDto, hotel);

        await hotelsRepository.UpdateAsync(hotel);

        return Result.Success();
    }

    public async Task<Result> DeleteHotelAsync(int id)
    {
        var hotel = await hotelsRepository.GetByIdAsync(id);

        if (hotel == null)
        {
            return Result.NotFound($"Hotel with id {id} was not found.");
        }

        try
        {
            await hotelsRepository.DeleteAsync(id);
        }
        catch (DbUpdateException)
        {
            return Result.Conflict($"Cannot delete Hotel with id {id} because it has existing Bookings. Please delete the bookings first.");
        }

        return Result.Success();
    }
}