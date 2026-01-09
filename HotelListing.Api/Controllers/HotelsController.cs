using HotelListing.Api.Models.DTOs.Hotel;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController(IHotelsService service) : ControllerBase
{
    // GET: api/Hotels
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetHotelsDto>>> GetHotels()
    {
        var response = await service.GetHotelsAsync();

        return Ok(response);
    }

    // GET: api/Hotels/5
    [HttpGet("{id}")]
    public async Task<ActionResult<GetHotelDto>> GetHotel(int id)
    {
        var response = await service.GetHotelAsync(id);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    // POST: api/Hotels
    [HttpPost]
    public async Task<ActionResult<GetHotelDto>> PostHotel(CreateHotelDto hotelDto)
    {
        var response = await service.CreateHotelAsync(hotelDto);

        return CreatedAtAction("GetHotel", new { id = response.Id }, response);
    }

    // PUT: api/Hotels/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutHotel(int id, UpdateHotelDto hotelDto)
    {
        if (id != hotelDto.Id)
        {
            return BadRequest();
        }

        var isUpdated = await service.UpdateHotelAsync(id, hotelDto);

        if (!isUpdated)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/Hotels/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var isDeleted = await service.DeleteHotelAsync(id);

        if (!isDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}