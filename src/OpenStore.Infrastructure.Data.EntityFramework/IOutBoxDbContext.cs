using Microsoft.EntityFrameworkCore;

namespace OpenStore.Infrastructure.Data.EntityFramework
{
    public interface IOutBoxDbContext
    {
        DbSet<OutBoxMessage> OutBoxMessages { get; set; }

        IOutBoxService OutBoxService { get; }
    }
}