using Microsoft.EntityFrameworkCore;
using OpenStore.Data.OutBox;
using OpenStore.Domain;

namespace OpenStore.Data.EntityFramework;

public interface IEntityFrameworkCoreUnitOfWork : IUnitOfWork
{
    DbContext Context { get; }
    IOutBoxStoreService OutBoxStoreService { get; }
}