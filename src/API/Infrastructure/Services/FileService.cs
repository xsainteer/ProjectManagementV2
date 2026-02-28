using Application.Interfaces.Services;
using Domain.Common;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _uploadPath;
    private readonly ILogger<FileService> _logger;

    public FileService(ILogger<FileService> logger)
    {
        _logger = logger;
        // Setting it to a folder in the current directory.
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        
        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    public async Task<Result<string>> SaveFileAsync(Stream fileStream, string fileName, string subFolder, CancellationToken cancellationToken = default)
    {
        try
        {
            var targetFolder = Path.Combine(_uploadPath, subFolder);
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            // Append a GUID to the fileName to prevent collisions
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(targetFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream, cancellationToken);
            }

            // Return the relative path from the root uploads folder
            return Result<string>.Success(Path.Combine(subFolder, uniqueFileName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving file {FileName}", fileName);
            return Result<string>.Failure(Error.Unexpected("An error occurred while saving the file."));
        }
    }

    public Result DeleteFile(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_uploadPath, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting file at {Path}", filePath);
            return Result.Failure(Error.Unexpected("An error occurred while deleting the file."));
        }
    }

    public Result<Stream> GetFileStream(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_uploadPath, filePath);
            if (!File.Exists(fullPath))
                return Result<Stream>.Failure(Error.NotFound("File not found on disk."));

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Result<Stream>.Success(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reading file at {Path}", filePath);
            return Result<Stream>.Failure(Error.Unexpected("An error occurred while reading the file."));
        }
    }

    public bool FileExists(string filePath)
    {
        var fullPath = Path.Combine(_uploadPath, filePath);
        return File.Exists(fullPath);
    }
}
