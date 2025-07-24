using CusotmFileUploader.Models;
using Microsoft.AspNetCore.Http;

namespace CusotmFileUploader.Services
{
    public interface IFileUploadService
    {
        Task<FileUploadResult> UploadFileAsync(IFormFile file, FileUploadOptions options);
        Task<List<FileUploadResult>> UploadFilesAsync(IFormFileCollection files, FileUploadOptions options);
        Task<bool> DeleteFileAsync(string fileId);
        Task<FileDownloadResult> DownloadFileAsync(string fileId);
    }
}
