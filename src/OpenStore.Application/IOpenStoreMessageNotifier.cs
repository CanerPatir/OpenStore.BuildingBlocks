using System.Threading.Tasks;

namespace OpenStore.Application
{
    public interface IOpenStoreMessageNotifier
    {
        Task Notify(MessageEnvelop outBoxMessage);
    }
}