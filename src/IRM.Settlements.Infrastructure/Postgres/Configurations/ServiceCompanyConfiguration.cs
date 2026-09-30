using IRM.Settlements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Settlements.Infrastructure.Postgres.Configurations;

internal sealed class ServiceCompanyConfiguration : IEntityTypeConfiguration<ServiceCompany>
{
    public void Configure(EntityTypeBuilder<ServiceCompany> entityBuilder)
    {
        entityBuilder.ToTable("ServiceCompanies");

        entityBuilder.HasKey(x => x.Id);

        entityBuilder.HasIndex(x => x.SapId).IsUnique();

        entityBuilder.Property(x => x.Name)
            .IsRequired();
    }
}
