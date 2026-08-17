namespace CareerProject.Shared.Storage;

// Pure validation logic, no IO - the resulting extension always comes from this whitelist,
// never from the client-supplied filename, so a malicious filename can't smuggle a path or an
// unexpected extension into storage.
public static class ImageUploadValidator
{
    private static readonly Dictionary<string, string> AllowedContentTypeToExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
    };

    public static bool TryValidate(string contentType, long lengthBytes, long maxBytes, out string extension, out string? error)
    {
        if (!AllowedContentTypeToExtension.TryGetValue(contentType, out var ext))
        {
            extension = "";
            error = $"Unsupported image type '{contentType}'. Allowed: JPEG, PNG, WEBP.";
            return false;
        }

        if (lengthBytes <= 0)
        {
            extension = "";
            error = "File is empty.";
            return false;
        }

        if (lengthBytes > maxBytes)
        {
            extension = "";
            error = $"File exceeds the maximum allowed size of {maxBytes / (1024 * 1024)} MB.";
            return false;
        }

        extension = ext;
        error = null;
        return true;
    }
}
