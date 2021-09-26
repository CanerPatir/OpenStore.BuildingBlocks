using System.Threading.Tasks;

namespace OpenStore.Data.OutBox
{
    public interface IOpenStoreOutBoxMessageNotifier
    {
        Task Notify(OutBoxMessage outBoxMessage);
    }
}