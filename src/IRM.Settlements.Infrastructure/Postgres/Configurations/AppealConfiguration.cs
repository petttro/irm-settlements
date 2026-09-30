using IRM.Settlements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Settlements.Infrastructure.Postgres.Configurations;

internal sealed class AppealConfiguration : IEntityTypeConfiguration<Appeal>
{
    public void Configure(EntityTypeBuilder<Appeal> builder)
    {
        builder.ToTable("Appeals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CouponNumber)
            .IsRequired();

        builder.HasIndex(x => x.CouponNumber)
            .IsUnique();

        builder.Property(x => x.AdditionalServices)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        builder.HasOne(x => x.ServiceCompany)
            .WithMany()
            .HasForeignKey(x => x.ServiceCompanyId);

        builder.HasOne(x => x.ServiceCenter)
            .WithMany()
            .HasForeignKey(x => x.ServiceCenterId);
    }
}
