using AutoMapper;
using HotelListing.Api.Models;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Hotel;
using HotelListing.Api.Models.Filtering;
using HotelListing.Api.Models.Pagination;
using HotelListing.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Services;

public class HotelsService(
    IHotelsRepository hotelsRepository,
    ICountriesRepository countriesRepository,
    IMapper mapper) : IHotelsService
{
    public async Task<Result<PagedResult<GetHotelsDto>>> GetHotelsAsync(
        PaginationParameters paging, HotelFilterParameters filters)
    {
        var query = hotelsRepository.GetAllAsQueryable();

        if (filters.CountryId.HasValue)
        {
            query = query.Where(x => x.CountryId == filters.CountryId.Value);
        }

        if (filters.MinRating.HasValue)
        {
            query = query.Where(x => x.Rating >= filters.MinRating.Value);
        }

        if (filters.MaxRating.HasValue)
        {
            query = query.Where(x => x.Rating <= filters.MaxRating.Value);
        }

        if (filters.MinPrice.HasValue)
        {
            query = query.Where(x => x.PerNightRate >= filters.MinPrice.Value);
        }

        if (filters.MaxPrice.HasValue)
        {
            query = query.Where(x => x.PerNightRate <= filters.MaxPrice.Value);
        }

        if (!string.IsNullOrEmpty(filters.Location))
        {
            query = query.Where(h => h.Address.Contains(filters.Location));
        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            query = query.Where(x =>
                x.Name.Contains(filters.Search) ||
                x.Address.Contains(filters.Search));
        }

        query = filters.SortBy?.ToLower() switch
        {
            "price" => filters.SortDescending
                ? query.OrderByDescending(x => x.PerNightRate).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.PerNightRate).ThenBy(x => x.Id),

            "rating" => filters.SortDescending
                ? query.OrderByDescending(x => x.Rating).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.Rating).ThenBy(x => x.Id),

            "name" => filters.SortDescending
                ? query.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.Name).ThenBy(x => x.Id),

            _ => query.OrderBy(x => x.Id)
        };

        var response = await hotelsRepository.GetAllPagedAsync<GetHotelsDto>(query, paging);

        return Result<PagedResult<GetHotelsDto>>.Success(response);
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

    public async Task<Result<GetAdminHotelDto>> AdminGetHotelAsync(int id)
    {
        var hotel = await hotelsRepository.GetHotelWithCountryAndAdminsAsync(id);

        if (hotel == null)
        {
            return Result<GetAdminHotelDto>.NotFound($"Hotel with id {id} was not found.");
        }

        var response = mapper.Map<GetAdminHotelDto>(hotel);

        return Result<GetAdminHotelDto>.Success(response);
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