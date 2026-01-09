using AutoMapper;
using HotelListing.Api.Data;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs;
using HotelListing.Api.Models.DTOs.Hotel;
using HotelListing.Api.Repositories.Implementation;
using HotelListing.Api.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController(IHotelsRepository repository, IMapper mapper) : ControllerBase
{

    // GET: api/Hotels
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetHotelsDto>>> GetHotels()
    {
        var hotels = await repository.GetAllAsync();

        var response = mapper.Map<List<GetHotelsDto>>(hotels);

        return Ok(response);
    }

    // GET: api/Hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        var hotel = await repository.GetHotelDetails(id);

        if (hotel == null)
        {
            return NotFound();
        }

        var response = mapper.Map<GetHotelDto>(hotel);

        return Ok(response);
    }

    // PUT: api/Hotels/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHotel(int id, UpdateHotelDto hotelDto)
    {
        if (id != hotelDto.Id)
        {
            return BadRequest();
        }

        var hotel = await repository.GetByIdAsync(id);

        if (hotel == null)
        {
            return NotFound();
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
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Hotels
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<GetHotelDto>> PostHotel(CreateHotelDto hotelDto)
    {
        var hotel = mapper.Map<Hotel>(hotelDto);

        await repository.AddAsync(hotel);

        var fullHotel = await repository.GetHotelDetails(hotel.Id);

        var createdHotelDto = mapper.Map<GetHotelDto>(fullHotel);

        return CreatedAtAction("GetHotel", new { id = hotel.Id }, createdHotelDto);
    }

    // DELETE: api/Hotels/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var hotel = await repository.GetByIdAsync(id);

        if (hotel == null)
        {
            return NotFound();
        }

        await repository.DeleteAsync(id);

        return NoContent();
    }
}