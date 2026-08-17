namespace CareerProject.Shared.Storage;

public interface IFileStorage
{
    // Returns a relative URL (e.g. "/uploads/photos/{name}") the caller can persist on the entity.
    Task<string> SaveAsync(Stream content, string fileExtension, string subfolder, CancellationToken cancellationToken = default);
}
