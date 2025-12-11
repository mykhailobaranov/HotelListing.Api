using HotelListing.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private static List<Hotel> hotels = new List<Hotel>
        {
        new Hotel { Id = 1, Name = "Grand Plaza", Address = "123 Main St", Rating = 4.5 },
        new Hotel { Id = 2, Name = "Ocean View", Address = "456 Beach Rd", Rating = 4.8 }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Hotel>> Get()
        {
            return Ok(hotels);
        }

        [HttpGet("{id}")]
        public ActionResult<Hotel> Get(int id)
        {
            var hotel = hotels.FirstOrDefault(hotel =>  hotel.Id == id);

            if (hotel == null)
            {
                return NotFound();
            }

            return Ok(hotel);
        }

        [HttpPost]
        public ActionResult<Hotel> Create(Hotel hotel)
        {
            var existedHotel = hotels.FirstOrDefault(exhotel => exhotel.Id == hotel.Id);
            if (existedHotel != null)
            {
                return BadRequest("Hotel with this Id already existed!");
            }
            hotels.Add(hotel);
            return CreatedAtAction(nameof(Get), new { id = hotel.Id }, hotel);
        }


        [HttpPut("{id}")]
        public ActionResult Update([FromBody] Hotel updatedHotel, int id)
        {
            var hotel = hotels.FirstOrDefault(hotel => hotel.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }

            hotel.Name = updatedHotel.Name;
            hotel.Address = updatedHotel.Address;
            hotel.Rating = updatedHotel.Rating;

            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var hotel = hotels.FirstOrDefault(exhotel =>  id == exhotel.Id);
            if(hotel == null)
            {
                return NotFound("Hotel not found");
            }

            hotels.Remove(hotel);
            return NoContent();
        }
    }
}
