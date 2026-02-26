using HotelListing.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Data.Configuration;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder
             .HasIndex(c => c.Name)
             .HasDatabaseName("IX_Countries_Name");

        builder
            .HasIndex(c => c.ShortName)
            .HasDatabaseName("IX_Countries_ShortName");
    }
}