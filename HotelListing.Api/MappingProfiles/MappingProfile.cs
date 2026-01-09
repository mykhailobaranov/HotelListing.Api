using AutoMapper;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Country;
using HotelListing.Api.Models.DTOs.Hotel;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace HotelListing.Api.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // === HOTELS ===

        CreateMap<Hotel, GetHotelDto>()
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src =>
                src.Country != null ? src.Country.Name : string.Empty
            ));

        CreateMap<Hotel, GetHotelsDto>();

        CreateMap<Hotel, GetHotelSlimDto>();

        CreateMap<CreateHotelDto, Hotel>();

        CreateMap<UpdateHotelDto, Hotel>();


        // === COUNTRIES ===

        CreateMap<Country, GetCountryDto>();

        CreateMap<Country, GetCountriesDto>();

        CreateMap<CreateCountryDto, Country>();

        CreateMap<UpdateCountryDto, Country>();
    }
}
