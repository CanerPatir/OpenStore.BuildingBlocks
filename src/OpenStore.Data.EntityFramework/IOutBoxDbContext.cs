using Microsoft.EntityFrameworkCore;
using OpenStore.Data.OutBox;

namespace OpenStore.Data.EntityFramework
{
    public interface IOutBoxDbContext
    {
        DbSet<OutBoxMessage> OutBoxMessages { get; set; }
    }
}