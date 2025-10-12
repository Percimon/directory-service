using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{

    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(x => x.Id.Value).HasName("id");
    }
}
