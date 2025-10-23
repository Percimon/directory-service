using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations
{
    public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
    {
        public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
        {
            builder.ToTable("department_positions");

            builder.HasKey(x => x.Id)
                .HasName("pk_department_positions");

            builder.Property(x => x.Id).HasColumnName("id");

            builder.Property(x => x.DepartmentId).HasColumnName("department_id");

            builder.Property(x => x.PositionId).HasColumnName("position_id");

            builder
                .HasOne(dp => dp.Department)
                .WithMany(d => d.DepartmentPositions)
                .HasForeignKey(dp => dp.DepartmentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne<Position>()
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}