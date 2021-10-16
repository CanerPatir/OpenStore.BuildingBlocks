using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenStore.Data.OutBox;

namespace OpenStore.Data.EntityFramework.EntityConfiguration;

public class OutBoxMessageEntityTypeConfiguration : IEntityTypeConfiguration<OutBoxMessage>
{
    public void Configure(EntityTypeBuilder<OutBoxMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AggregateId).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.Committed).IsRequired();
        builder.Property(x => x.Timestamp).IsRequired();
        builder.Property(x => x.Version).IsRequired().IsConcurrencyToken();
    }
}