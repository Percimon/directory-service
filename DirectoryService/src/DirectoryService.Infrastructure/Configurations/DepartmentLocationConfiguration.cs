using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations
{
    public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
    {
        public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
        {
            builder.ToTable("department_locations");

            builder.HasKey(x => x.Id)
                .HasName("pk_department_locations");

            builder.Property(x => x.Id).HasColumnName("id");

            builder.Property(x => x.DepartmentId).HasColumnName("department_id");

            builder.Property(x => x.LocationId).HasColumnName("location_id");

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

        }
    }
}