namespace OpenStore.Data.EventSourcing.EventStore;

public interface ISerializer
{
    byte[] Serialize(object data);
    T Deserialize<T>(byte[] data);
        
    object Deserialize(byte[] data, Type returnType);
}