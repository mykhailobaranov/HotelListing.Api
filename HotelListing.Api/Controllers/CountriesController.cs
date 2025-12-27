﻿using HotelListing.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.Api.Models.DTOs;
using HotelListing.Api.Models.DTOs.Country;
using HotelListing.Api.Models.DTOs.Hotel;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountriesController(HotelListingDbContext context) : ControllerBase
{
    // GET: api/Countries
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCountriesDto>>> GetCountries()
    {
        var countries = await context.Countries
            .Select(c=> new GetCountriesDto(
                c.CountryId,
                c.Name,
                c.ShortName
                ))
            .ToListAsync();

        return Ok(countries);
    }

    // GET: api/Countries/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetCountryDto>> GetCountry(int id)
    {
        var country = await context.Countries
            .Where(c=>c.CountryId == id)
            .Select(c=> new GetCountryDto(
                c.CountryId,
                c.Name,
                c.ShortName,
                c.Hotels.Select(h=>new GetHotelSlimDto(
                    h.Id,
                    h.Name,
                    h.Address,
                    h.Rating
                )).ToList()
            ))
            .FirstOrDefaultAsync();

        if (country == null)
        {
            return NotFound();
        }

        return Ok(country);
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

        var country = await context.Countries.FindAsync(id);

        if (country == null)
        {
            return NotFound();
        }

        country.ShortName = countryDto.ShortName;
        country.Name = countryDto.Name;
       
        context.Entry(country).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await CountryExistsAsync(id))
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
        context.Countries.Add(country);
        await context.SaveChangesAsync();

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
        var country = await context.Countries.FindAsync(id);
        if (country == null)
        {
            return NotFound();
        }

        context.Countries.Remove(country);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> CountryExistsAsync(int id)
    {
        return await context.Countries.AnyAsync(e => e.CountryId == id);
    }
}