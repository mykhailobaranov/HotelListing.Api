using AutoMapper;
using HotelListing.Api.Models;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Country;
using HotelListing.Api.Models.Filtering;
using HotelListing.Api.Models.Pagination;
using HotelListing.Api.Repositories.Interface;
using Humanizer;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
namespace HotelListing.Api.Services;

public class CountriesService(ICountriesRepository repository, IMapper mapper) : ICountriesService
{
    public async Task<Result<PagedResult<GetCountriesDto>>> GetCountriesAsync(PaginationParameters paging, CountryFilterParameters filters)
    {
        var query = repository.GetAllAsQueryable();

        if (!string.IsNullOrEmpty(filters.Search))
        {
            query = query.Where(c => c.Name.Contains(filters.Search));
        }

        query = filters.SortBy?.ToLower() switch
        {
            "name" => filters.SortDescending
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),

            _ => query.OrderBy(x => x.Name)
        };

        var response = await repository.GetAllPagedAsync<GetCountriesDto>(query, paging);

        return Result<PagedResult<GetCountriesDto>>.Success(response);
    }

    public async Task<Result<GetCountryDto>> GetCountryAsync(int id)
    {
        var country = await repository.GetCountryWithHotelsAsync(id);

        if (country == null)
        {
            return Result<GetCountryDto>.NotFound($"Country with id {id} was not found.");
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

        var response = mapper.Map<GetCountryDto>(country);

        return Result<GetCountryDto>.Success(response);
    }

    public async Task<Result> UpdateCountryAsync(int id, UpdateCountryDto countryDto)
    {
        if (id != countryDto.CountryId)
        {
            return Result.BadRequest($"Route Id: {id} doesn't match Body Id: {countryDto.CountryId}.");
        }

        var country = await repository.GetByIdAsync(id);

        if (country == null)
        {
            return Result.NotFound($"Country with id {id} was not found.");
        }

        var normalizedName = countryDto.Name.ToLower().Trim();
        var duplicateExists = await repository.ExistsAsync(c => c.Name.ToLower().Trim() == normalizedName
                    && c.CountryId != id);

        if(duplicateExists)
        {
            return Result.Conflict($"Country with name '{countryDto.Name}' already exists.");
        }

        mapper.Map(countryDto, country);

        await repository.UpdateAsync(country);

        return Result.Success();
    }

    public async Task<Result> DeleteCountryAsync(int id)
    {
        var country = await repository.GetByIdAsync(id);

        if (country == null)
        {
            return Result.NotFound($"Country with id {id} was not found.");
        }

        try
        {
            await repository.DeleteAsync(id);
        }
        catch (DbUpdateException)
        {
            return Result.Conflict($"Cannot delete Country with id {id} because it has associated Hotels. Please delete the hotels first.");
        }

        return Result.Success();
    }

    public async Task<Result> PatchCountryAsync(int id, JsonPatchDocument<UpdateCountryDto> patchDoc)
    {
        var country = await repository.GetByIdAsync(id);

        if (country == null)
        {
            return Result.NotFound($"Country with id {id} was not found.");
        }

        var countryDto = mapper.Map<UpdateCountryDto>(country);
        patchDoc.ApplyTo(countryDto);

        if (countryDto.CountryId != id || countryDto.ShortName.Length > 3 || countryDto.Name.Length > 50)
        {
            return Result.BadRequest("Invalid data: route Id must match CountryId, " +
                "ShortName must be at most 3 characters long, " +
                "and Name must be at most 50 characters long.");
        }

        var normalizedName = countryDto.Name.ToLower().Trim();
        var duplicateExists = await repository.ExistsAsync(c => c.Name.ToLower().Trim() == normalizedName
                    && c.CountryId != id);

        if (duplicateExists)
        {
            return Result.Conflict($"Country with name '{countryDto.Name}' already exists.");
        }

        mapper.Map(countryDto, country);
        await repository.UpdateAsync(country);

        return Result.Success();
    }
}