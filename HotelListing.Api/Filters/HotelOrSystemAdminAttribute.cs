using HotelListing.Api.Models.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HotelListing.Api.AuthorizationFilters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class HotelOrSystemAdminAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpUser = context.HttpContext.User;

        if (httpUser?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        if (httpUser.IsInRole(RoleNames.Admin))
        {
            return Task.CompletedTask;
        }

        if (!httpUser.IsInRole(RoleNames.HotelAdmin))
        {
            context.Result = new ForbidResult();
            return Task.CompletedTask;
        }

        context.RouteData.Values.TryGetValue("hotelId", out var hotelIdObj);
        if (!int.TryParse(hotelIdObj?.ToString(), out int routeHotelId) || routeHotelId == 0)
        {
            context.Result = new ForbidResult();
            return Task.CompletedTask;
        }

        var hasAccessToHotel = httpUser.FindAll("HotelId")
                                       .Any(c => c.Value == routeHotelId.ToString());

        if (!hasAccessToHotel)
        {
            context.Result = new ForbidResult();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}