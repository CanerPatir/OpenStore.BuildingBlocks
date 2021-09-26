using Microsoft.EntityFrameworkCore;

namespace OpenStore.Data.EntityFramework
{
    public interface IOutBoxDbContext
    {
        DbSet<OutBoxMessage> OutBoxMessages { get; set; }
    }
}