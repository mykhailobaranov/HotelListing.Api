using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/v{version:apiVersion}/countries")]
[ApiVersion("2.0", Deprecated = true)]
[ApiController]
public class CountriesV2Controller : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Message = "Welcome to the V2 API!",
            Version = "2.0",
            Data = "No data, just testing purposes ;D"
        });
    }
}