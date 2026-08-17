namespace CareerProject.Shared.Storage;

// Development-only storage: files land under {contentRoot}/wwwroot/uploads/{subfolder}, served
// back out via app.UseStaticFiles(). Swapping to S3-compatible storage later only means writing
// a new IFileStorage implementation - callers never see the difference.
public class LocalFileStorage(string webRootPath) : IFileStorage
{
    public async Task<string> SaveAsync(Stream content, string fileExtension, string subfolder, CancellationToken cancellationToken = default)
    {
        var uploadsRoot = Path.Combine(webRootPath, "uploads", subfolder);
        Directory.CreateDirectory(uploadsRoot);

        // Never trust the caller's original filename as a storage key (path traversal, collisions).
        var fileName = $"{Guid.NewGuid():N}{fileExtension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using var fileStream = new FileStream(filePath, FileMode.CreateNew);
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"/uploads/{subfolder}/{fileName}";
    }
}
