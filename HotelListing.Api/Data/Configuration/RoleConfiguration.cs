using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelListing.Api.Data.Configuration;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new IdentityRole
            {
                Id = "cac43a6e-f7bb-4448-baaf-1add431ccbbf",
                Name = "Admin",
                NormalizedName = "ADMIN",
            },

            new IdentityRole
            {
                Id = "cbc43a6e-f7bb-4448-baaf-1add431ccbbf",
                Name = "User",
                NormalizedName = "USER"
            }
            );
    }
}
