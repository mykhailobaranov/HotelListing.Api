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
            ErrorType.BadRequest => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad request",
                detail: error
            ),
            ErrorType.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource not found",
                detail: error
            ),
            ErrorType.Conflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: error
            ),
            _ => Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal server error",
                detail: error
            )
        };
    }
}