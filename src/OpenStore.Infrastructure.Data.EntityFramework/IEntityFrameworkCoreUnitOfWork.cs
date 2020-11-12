using Microsoft.EntityFrameworkCore;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.Data.EntityFramework
{
    public interface IEntityFrameworkCoreUnitOfWork : IUnitOfWork
    {
        DbContext Context { get; }
    }
}