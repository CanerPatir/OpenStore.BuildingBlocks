namespace OpenStore.Infrastructure.Web.ErrorHandling;

internal record OpenStoreWebErrorDto(string Message, IEnumerable<string> Errors);