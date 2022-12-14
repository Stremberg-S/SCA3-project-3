using FFMP.Models;

namespace FFMP.BlobStorageServices
{
    public interface IBlobStorageService
    {
        Task<List<BlobStorage>> GetAllBlobFiles();
        Task UploadBlobFileAsync(IFormFile files);
        Task DeleteDocumentAsync(string blobName);
        Task DownloadDocumentAsync(string blobName);
    }
}
