using LaboratorioTlahuac.Application.Catalog;

namespace LaboratorioTlahuac.Infrastructure.Catalog;

public sealed class CatalogImageStorage(CatalogImagesOptions options) : ICatalogImageStorage
{
    private const int CopyBufferSize = 81_920;
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8];

    private static readonly IReadOnlyDictionary<string, string> ContentTypeByExtension =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [".webp"] = "image/webp",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png"
        };

    public async Task<CatalogImageStoreResult> StoreAsync(
        CatalogImageUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateMetadata(request);

        if (validation.Result is not null)
        {
            return validation.Result;
        }

        if (!TryGetStorageRoot(out var storageRoot))
        {
            return Unavailable();
        }

        var header = new byte[12];
        int headerLength;

        try
        {
            headerLength = await ReadHeaderAsync(request.Content, header, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException)
        {
            return Validation("The file could not be read.");
        }

        if (!HasExpectedSignature(validation.Extension!, header.AsSpan(0, headerLength)))
        {
            return Validation("The file signature does not match its image format.");
        }

        var fileName = CatalogImageFileName.Create(validation.Extension!);
        var temporaryFileName = $".upload-{Guid.NewGuid():N}.tmp";

        if (!TryResolvePath(storageRoot, fileName, out var finalPath)
            || !TryResolvePath(storageRoot, temporaryFileName, out var temporaryPath))
        {
            return Unavailable();
        }

        var finalCreated = false;

        try
        {
            await using (var output = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                CopyBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await output.WriteAsync(header.AsMemory(0, headerLength), cancellationToken);
                var totalBytes = (long)headerLength;
                var buffer = new byte[CopyBufferSize];

                while (true)
                {
                    var bytesRead = await request.Content.ReadAsync(buffer, cancellationToken);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    totalBytes += bytesRead;

                    if (totalBytes > CatalogImagesOptions.MaximumFileSizeBytes)
                    {
                        return TooLarge();
                    }

                    await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                }

                await output.FlushAsync(cancellationToken);
            }

            File.Move(temporaryPath, finalPath, overwrite: false);
            finalCreated = true;

            return new CatalogImageStoreResult(
                CatalogImageStoreStatus.Success,
                fileName,
                new Dictionary<string, string[]>());
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException)
        {
            return Unavailable();
        }
        catch (UnauthorizedAccessException)
        {
            return Unavailable();
        }
        finally
        {
            TryDeleteFile(temporaryPath);

            if (!finalCreated)
            {
                TryDeleteFile(finalPath);
            }
        }
    }

    public Task<CatalogImageContent?> OpenReadAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (!CatalogImageFileName.IsGeneratedName(fileName)
            || !TryGetStorageRoot(out var storageRoot)
            || !TryResolvePath(storageRoot, fileName, out var path)
            || !File.Exists(path))
        {
            return Task.FromResult<CatalogImageContent?>(null);
        }

        try
        {
            var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                CopyBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var contentType = CatalogImageFileName.GetContentType(fileName);

            if (contentType is null)
            {
                stream.Dispose();
                return Task.FromResult<CatalogImageContent?>(null);
            }

            return Task.FromResult<CatalogImageContent?>(new CatalogImageContent(stream, contentType));
        }
        catch (IOException)
        {
            return Task.FromResult<CatalogImageContent?>(null);
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult<CatalogImageContent?>(null);
        }
    }

    public Task TryDeleteAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (CatalogImageFileName.IsGeneratedName(fileName)
            && TryGetStorageRoot(out var storageRoot)
            && TryResolvePath(storageRoot, fileName, out var path))
        {
            TryDeleteFile(path);
        }

        return Task.CompletedTask;
    }

    private static (string? Extension, CatalogImageStoreResult? Result) ValidateMetadata(
        CatalogImageUploadRequest request)
    {
        if (request.Length == 0)
        {
            return (null, Validation("The file cannot be empty."));
        }

        if (request.Length < 0)
        {
            return (null, Validation("The file length is invalid."));
        }

        if (request.Length > CatalogImagesOptions.MaximumFileSizeBytes)
        {
            return (null, TooLarge());
        }

        string originalFileName;
        string extension;
        string fileNameWithoutExtension;

        try
        {
            originalFileName = Path.GetFileName(request.FileName);
            extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        }
        catch (Exception exception) when (
            exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return (null, Validation("The file name is invalid."));
        }

        if (string.IsNullOrEmpty(extension)
            || string.IsNullOrEmpty(fileNameWithoutExtension)
            || !ContentTypeByExtension.TryGetValue(extension, out var expectedContentType))
        {
            return (null, Validation("The file extension is not allowed."));
        }

        var contentType = request.ContentType.Trim();

        if (!ContentTypeByExtension.Values.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            return (null, Validation("The file MIME type is not allowed."));
        }

        if (!string.Equals(contentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
        {
            return (null, Validation("The file extension and MIME type do not match."));
        }

        return (extension, null);
    }

    private bool TryGetStorageRoot(out string storageRoot)
    {
        storageRoot = string.Empty;

        if (string.IsNullOrWhiteSpace(options.StoragePath)
            || !Path.IsPathFullyQualified(options.StoragePath))
        {
            return false;
        }

        try
        {
            storageRoot = Path.GetFullPath(options.StoragePath);
            return Directory.Exists(storageRoot);
        }
        catch (Exception exception) when (
            exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static bool TryResolvePath(string storageRoot, string fileName, out string path)
    {
        path = string.Empty;

        try
        {
            var canonicalRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(storageRoot));
            var rootPrefix = canonicalRoot + Path.DirectorySeparatorChar;
            var candidate = Path.GetFullPath(Path.Combine(canonicalRoot, fileName));
            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (!candidate.StartsWith(rootPrefix, comparison))
            {
                return false;
            }

            path = candidate;
            return true;
        }
        catch (Exception exception) when (
            exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static async Task<int> ReadHeaderAsync(
        Stream content,
        byte[] header,
        CancellationToken cancellationToken)
    {
        var totalRead = 0;

        while (totalRead < header.Length)
        {
            var bytesRead = await content.ReadAsync(
                header.AsMemory(totalRead, header.Length - totalRead),
                cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            totalRead += bytesRead;
        }

        return totalRead;
    }

    private static bool HasExpectedSignature(string extension, ReadOnlySpan<byte> header)
    {
        return extension switch
        {
            ".png" => header.StartsWith(PngSignature),
            ".jpg" or ".jpeg" => header.StartsWith(JpegSignature),
            ".webp" => header.Length >= 12
                && header[..4].SequenceEqual("RIFF"u8)
                && header.Slice(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }

    private static CatalogImageStoreResult Validation(string error)
    {
        return new CatalogImageStoreResult(
            CatalogImageStoreStatus.ValidationError,
            null,
            new Dictionary<string, string[]> { ["file"] = [error] });
    }

    private static CatalogImageStoreResult TooLarge()
    {
        return new CatalogImageStoreResult(
            CatalogImageStoreStatus.PayloadTooLarge,
            null,
            new Dictionary<string, string[]>());
    }

    private static CatalogImageStoreResult Unavailable()
    {
        return new CatalogImageStoreResult(
            CatalogImageStoreStatus.ServiceUnavailable,
            null,
            new Dictionary<string, string[]>());
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
