using HotelListing.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Data.Configuration;

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder
            .HasOne(h => h.Country)
            .WithMany(c => c.Hotels)
            .HasForeignKey(h => h.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
           .HasIndex(h => h.Name)
           .HasDatabaseName("IX_Hotels_Name");

        builder
            .HasIndex(h => new { h.CountryId, h.Rating })
            .HasDatabaseName("IX_Hotels_CountryId_Rating");
    }
}