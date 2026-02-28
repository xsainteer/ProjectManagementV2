using Domain.Common;

namespace Application.Interfaces.Services;

public interface IFileService
{
    Task<Result<string>> SaveFileAsync(Stream fileStream, string fileName, string subFolder, CancellationToken cancellationToken = default);
    Result DeleteFile(string filePath);
    Result<Stream> GetFileStream(string filePath);
    bool FileExists(string filePath);
}
