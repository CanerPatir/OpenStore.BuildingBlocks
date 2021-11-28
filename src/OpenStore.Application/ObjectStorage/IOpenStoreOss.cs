namespace OpenStore.Application.ObjectStorage;

public interface IOpenStoreOss
{
    /// <summary>
    /// Uploads data to a blob from stream.
    /// overwritten.
    /// </summary>
    /// <param name="fullPath">Blob metadata</param>
    /// <param name="dataStream">Stream to upload from</param>
    /// <param name="cancellationToken"></param>
    /// <param name="append">When true, appends to the file instead of writing a new one.</param>
    /// <returns>Writeable stream</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when any parameter is null</exception>
    /// <exception cref="T:System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
    Task WriteAsync(string fullPath, Stream dataStream, bool append = false, CancellationToken cancellationToken = default);

    /// <summary>Opens the blob stream to read.</summary>
    /// <param name="fullPath">Blob's full path</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Stream in an open state, or null if blob doesn't exist by this ID. It is your responsibility to close and dispose this
    /// stream after use.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when any parameter is null</exception>
    /// <exception cref="T:System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
    Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default);

    /// <summary>Deletes an object by it's full path.</summary>
    /// <param name="fullPaths">Path to delete. If this path points to a folder, the folder is deleted recursively.</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when ID is null.</exception>
    /// <exception cref="T:System.ArgumentException">Thrown when ID is too long. Long IDs are the ones longer than 50 characters.</exception>
    Task DeleteAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default);

    /// <summary>Checksi if blobs exists in the storage</summary>
    /// <param name="fullPaths">List of paths to blobs</param>
    /// <param name="cancellationToken"></param>
    /// <returns>List of results of true and false indicating existence</returns>
    Task<IReadOnlyCollection<bool>> ExistsAsync(IEnumerable<string> fullPaths, CancellationToken cancellationToken = default);
}