using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;

namespace DirectoryService.Infrastructure.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.ToTable("locations");

            builder.HasKey(x => x.Id).HasName("pk_locations");

            builder
                .Property(x => x.Id)
                .HasConversion(x => x.Value, id => LocationId.Create(id))
                .HasColumnName("id");

            builder.ComplexProperty(x => x.Name, nb =>
            {
                nb.Property(name => name.Value)
                    .IsRequired()
                    .HasMaxLength(Constants.TextLength.LENGTH_150)
                    .HasColumnName("name");
            });

            builder.ComplexProperty(x => x.Address, ab =>
            {
                ab.Property(a => a.City)
                    .IsRequired()
                    .HasMaxLength(Constants.TextLength.LENGTH_150)
                    .HasColumnName("city");

                ab.Property(a => a.District)
                    .IsRequired()
                    .HasMaxLength(Constants.TextLength.LENGTH_150)
                    .HasColumnName("district");

                ab.Property(a => a.Street)
                    .IsRequired()
                    .HasMaxLength(Constants.TextLength.LENGTH_150)
                    .HasColumnName("street");

                ab.Property(a => a.Structure)
                    .IsRequired()
                    .HasMaxLength(Constants.TextLength.LENGTH_150)
                    .HasColumnName("structure");
            });

            builder.ComplexProperty(x => x.TimeZone, tb =>
            {
                tb.Property(t => t.Value)
                    .IsRequired()
                    .HasMaxLength(Constants.TextLength.LENGTH_50)
                    .HasColumnName("timezone");
            });

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.Property(p => p.UpdatedAt)
                .HasColumnName("updated_at");
        }
    }
}