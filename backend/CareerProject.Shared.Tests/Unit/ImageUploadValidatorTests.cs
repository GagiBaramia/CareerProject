using CareerProject.Shared.Storage;
using Xunit;

namespace CareerProject.Shared.Tests.Unit;

public class ImageUploadValidatorTests
{
    private const long MaxBytes = 5 * 1024 * 1024;

    [Theory]
    [InlineData("image/jpeg", ".jpg")]
    [InlineData("image/png", ".png")]
    [InlineData("image/webp", ".webp")]
    public void TryValidate_AllowedContentType_ReturnsTrueWithMatchingExtension(string contentType, string expectedExtension)
    {
        var isValid = ImageUploadValidator.TryValidate(contentType, 1024, MaxBytes, out var extension, out var error);

        Assert.True(isValid);
        Assert.Equal(expectedExtension, extension);
        Assert.Null(error);
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/gif")]
    [InlineData("text/plain")]
    public void TryValidate_DisallowedContentType_ReturnsFalse(string contentType)
    {
        var isValid = ImageUploadValidator.TryValidate(contentType, 1024, MaxBytes, out _, out var error);

        Assert.False(isValid);
        Assert.NotNull(error);
    }

    [Fact]
    public void TryValidate_ExceedsMaxSize_ReturnsFalse()
    {
        var isValid = ImageUploadValidator.TryValidate("image/png", MaxBytes + 1, MaxBytes, out _, out var error);

        Assert.False(isValid);
        Assert.NotNull(error);
    }

    [Fact]
    public void TryValidate_EmptyFile_ReturnsFalse()
    {
        var isValid = ImageUploadValidator.TryValidate("image/png", 0, MaxBytes, out _, out var error);

        Assert.False(isValid);
        Assert.NotNull(error);
    }

    [Fact]
    public void TryValidate_ExactlyAtMaxSize_ReturnsTrue()
    {
        var isValid = ImageUploadValidator.TryValidate("image/png", MaxBytes, MaxBytes, out _, out var error);

        Assert.True(isValid);
        Assert.Null(error);
    }
}
