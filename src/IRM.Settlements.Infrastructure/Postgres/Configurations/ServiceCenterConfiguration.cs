using IRM.Settlements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Settlements.Infrastructure.Postgres.Configurations;

internal sealed class ServiceCenterConfiguration : IEntityTypeConfiguration<ServiceCenter>
{
    public void Configure(EntityTypeBuilder<ServiceCenter> entityBuilder)
    {
        entityBuilder.ToTable("ServiceCenters");

        entityBuilder.HasKey(x => x.Id);

        entityBuilder.HasIndex(i => i.ServiceCompanySapId);

        entityBuilder.HasIndex(x => x.ExternalId).IsUnique();

        entityBuilder.Property(x => x.Name)
            .IsRequired();
    }
}
