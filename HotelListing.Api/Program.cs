using HotelListing.Api.Data;
using HotelListing.Api.MappingProfiles;
using HotelListing.Api.Models.Domain;
using HotelListing.Api.Repositories.Implementation;
using HotelListing.Api.Repositories.Interface;
using HotelListing.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("HotelListingDbConnectionString");
builder.Services.AddDbContext<HotelListingDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddControllers().AddJsonOptions(opt=>
{
    opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddIdentityCore<ApplicationUser>(options => { })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<HotelListingDbContext>();

builder.Services.AddOpenApi();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IHotelsRepository, HotelsRepository>();
builder.Services.AddScoped<IHotelsService, HotelsService>();
builder.Services.AddScoped<ICountriesService, CountriesService>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
