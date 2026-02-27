using AutoMapper;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Models.DTOs.Booking;
using HotelListing.Api.Models.DTOs.Country;
using HotelListing.Api.Models.DTOs.Hotel;

namespace HotelListing.Api.MappingProfiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // === HOTELS ===

        CreateMap<Hotel, GetHotelDto>()
            .ForMember(d => d.Country,
                opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : string.Empty));

        CreateMap<Hotel, GetHotelsDto>();

        CreateMap<Hotel, GetHotelSlimDto>();

        CreateMap<CreateHotelDto, Hotel>();

        CreateMap<UpdateHotelDto, Hotel>();


        // === COUNTRIES ===

        CreateMap<Country, GetCountryDto>();

        CreateMap<Country, GetCountriesDto>();

        CreateMap<CreateCountryDto, Country>();

        CreateMap<UpdateCountryDto, Country>().ReverseMap();


        // === BOOKINGS ===

        CreateMap<Booking, GetBookingDto>();

        CreateMap<Booking, GetBookingDetailsDto>()
            .ForMember(d => d.HotelAddress,
                opt => opt.MapFrom(src => src.Hotel!.Address ?? "Unknown"))
            .ForCtorParam("HotelCountry", opt =>
                opt.MapFrom(src => src.Hotel!.Country!.Name ?? "Unknown"));

        CreateMap<Booking, GetAdminBookingDetailsDto>()
            .ForCtorParam("GuestFullName",
                opt => opt.MapFrom(src => src.User!.FullName))
            .ForCtorParam("GuestEmail",
                opt => opt.MapFrom(src => src.User!.Email ?? "Unknown"));

        CreateMap<CreateBookingDto, Booking>();
    }
}