using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenStore.Domain;

namespace OpenStore.Data.EntityFramework.EntityConfiguration
{
    public abstract class BaseEntityTypeConfiguration<TKey, TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : Entity<TKey>
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Version).IsConcurrencyToken();
        }
    }
    
}