using IRM.Settlements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Settlements.Infrastructure.Postgres.Configurations;

internal sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        builder.HasKey(r => r.Id);

        builder.HasIndex(r => new {r.ServiceCompanySapId, r.CreatedAt}).IsDescending(false, true);

        builder.Property(x => x.ServiceCenterExternalIds)
            .HasColumnType("text[]")
            .HasDefaultValueSql("'{}'");

        builder
            .HasMany(r => r.Items)
            .WithOne(i => i.Report)
            .HasForeignKey(i => i.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
