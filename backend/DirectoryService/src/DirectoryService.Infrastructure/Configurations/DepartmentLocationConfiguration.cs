using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(x => x.Id)
            .HasName("pk_department_locations");

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.DepartmentId)
            .HasConversion(id => id.Value, value => DepartmentId.Create(value))
            .HasColumnName("department_id");

        builder.Property(x => x.LocationId)
            .HasConversion(id => id.Value, value => LocationId.Create(value))
            .HasColumnName("location_id");

        builder.Property(x => x.IsPrimary).HasColumnName("is_primary");

        builder
            .HasOne(dl => dl.Department)
            .WithMany(d => d.DepartmentLocations)
            .HasForeignKey(dl => dl.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Location>()
            .WithMany()
            .HasForeignKey(x => x.LocationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.Department.IsActive);

    }
}