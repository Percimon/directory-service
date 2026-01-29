using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedService.SharedKernel;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(x => x.Id).HasName("pk_positions");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => PositionId.Create(id))
            .HasColumnName("id");

        builder.OwnsOne(x => x.Name, nb =>
        {
            nb.Property(n => n.Value)
                .IsRequired()
                .HasMaxLength(Constants.TextLength.LENGTH_50)
                .HasColumnName("name");

            nb.HasIndex(x => x.Value)
                .IsUnique()
                .HasDatabaseName("ix_positions_name");
        });

        builder.ComplexProperty(x => x.Description, db =>
        {
            db.Property(d => d.Value)
                .IsRequired()
                .HasMaxLength(Constants.TextLength.LENGTH_500)
                .HasColumnName("description");
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