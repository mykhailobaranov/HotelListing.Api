﻿using HotelListing.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.Api.Models.DTOs;
using HotelListing.Api.Models.DTOs.Country;
using HotelListing.Api.Models.DTOs.Hotel;
using HotelListing.Api.Repositories.Interface;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController(ICountriesRepository repository) : ControllerBase
{
    // GET: api/Countries
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries()
    {
        var countries = await repository.GetAllAsync();

        var response = countries.Select(c => new GetCountriesDto(
            c.CountryId,
            c.Name,
            c.ShortName
            )).ToList();

        return Ok(response);
    }

    // GET: api/Countries/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int id)
    {
        var country = await repository.GetCountryDetails(id);

        if (country == null)
        {
            return NotFound();
        }

        var response = new GetCountryDto(
            country.CountryId,
            country.Name,
            country.ShortName,
            country.Hotels.Select(h => new GetHotelSlimDto(
                    h.Id,
                    h.Name,
                    h.Address,
                    h.Rating
                )).ToList());

        return Ok(response);
    }

    // PUT: api/Countries/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCountry(int id, UpdateCountryDto countryDto)
    {
        if (id != countryDto.Id)
        {
            return BadRequest();
        }

        var country = await repository.GetByIdAsync(id);

        if (country == null)
        {
            return NotFound();
        }

        country.ShortName = countryDto.ShortName;
        country.Name = countryDto.Name;

        try
        {
            await repository.UpdateAsync(country);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await repository.Exists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Countries
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<GetCountryDto>> PostCountry(CreateCountryDto countryDto)
    {
        var country = new Country
        {
            ShortName = countryDto.ShortName,
            Name = countryDto.Name,
        };

        await repository.AddAsync(country);

        var createdCountryDto = new GetCountryDto(
            country.CountryId,
            country.Name,
            country.ShortName,
            []
            );

        return CreatedAtAction("GetCountry", new { id = country.CountryId }, createdCountryDto);
    }

    // DELETE: api/Countries/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var country = await repository.GetByIdAsync(id);
        if (country == null)
        {
            return NotFound();
        }

        await repository.DeleteAsync(id);

        return NoContent();
    }
}