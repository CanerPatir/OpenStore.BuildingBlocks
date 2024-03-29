using MediatR;

namespace OpenStore.Data.OutBox;

public record OutBoxMessageBatch(IReadOnlyCollection<OutBoxMessage> Messages) : INotification;