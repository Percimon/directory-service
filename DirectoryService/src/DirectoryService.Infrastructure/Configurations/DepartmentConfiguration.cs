using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SharedKernel;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(x => x.Id).HasName("pk_departments");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => DepartmentId.Create(value))
            .HasColumnName("id");

        builder.ComplexProperty(x => x.Name, nb =>
               {
                   nb.Property(name => name.Value)
                       .IsRequired()
                       .HasMaxLength(Constants.TextLength.LENGTH_50)
                       .HasColumnName("name");
               });

        builder.ComplexProperty(x => x.Identifier, tb =>
                {
                    tb.Property(d => d.Value)
                        .IsRequired()
                        .HasMaxLength(Constants.TextLength.LENGTH_150)
                        .HasColumnName("identifier");
                });

        builder.ComplexProperty(x => x.Path, tb =>
        {
            tb.Property(d => d.Value)
                .IsRequired()
                .HasMaxLength(Constants.TextLength.LENGTH_150)
                .HasColumnName("path");
        });

        builder.ComplexProperty(x => x.Depth, tb =>
        {
            tb.Property(d => d.Value)
                .IsRequired()
                .HasColumnName("depth");
        });

        builder.Property(x => x.IsActive)
               .IsRequired()
               .HasColumnName("is_active");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey("fk_parent_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
