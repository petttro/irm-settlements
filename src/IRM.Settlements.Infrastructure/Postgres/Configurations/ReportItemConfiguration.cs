using IRM.Settlements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Settlements.Infrastructure.Postgres.Configurations;

internal sealed class ReportItemConfiguration : IEntityTypeConfiguration<ReportItem>
{
    public void Configure(EntityTypeBuilder<ReportItem> builder)
    {
        builder.ToTable("ReportItems");
        builder.HasKey(i => i.Id);
        builder.Property(x => x.AdditionalServices)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");
        builder.HasIndex(i => i.CouponNumber).IsUnique();
        builder.HasIndex(i => new { i.ReportId, i.ServiceDate });

        builder.HasOne<MvzItem>(i => i.Mvz)
            .WithMany()
            .HasForeignKey(x => x.ShopName)
            .HasPrincipalKey(m => m.Id);
    }
}
