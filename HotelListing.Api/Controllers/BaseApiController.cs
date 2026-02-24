using HotelListing.Api.Models;
using HotelListing.Api.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseApiController : ControllerBase
{
    protected ActionResult<T> HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return MapError(result.ErrorType, result.Error);
    }

    protected ActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return MapError(result.ErrorType, result.Error);
    }

    protected ActionResult MapError(ErrorType errorType, string error)
    {
        return errorType switch
        {
            ErrorType.NotFound => NotFound(new { message = error }),
            ErrorType.BadRequest => BadRequest(new { message = error }),
            ErrorType.Conflict => Conflict(new { message = error }),
            _ => StatusCode(500, new { message = error })
        };
    }
}