using IRM.Settlements.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IRM.Settlements.Infrastructure.Postgres.Configurations;

internal sealed class MvzConfiguration : IEntityTypeConfiguration<MvzItem>
{
    public void Configure(EntityTypeBuilder<MvzItem> entityBuilder)
    {
        entityBuilder.ToTable("Mvz");

        entityBuilder.HasKey(x => x.Id);
    }
}
