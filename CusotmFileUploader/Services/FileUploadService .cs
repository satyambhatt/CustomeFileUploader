using CusotmFileUploader.Attributes;
using CusotmFileUploader.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusotmFileUploader.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly DbContext _dbContext;
        private readonly ILogger<FileUploadService> _logger;

        public FileUploadService(DbContext dbContext, ILogger<FileUploadService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<FileDownloadResult> DownloadFileAsync(string fileId)
        {
            try
            {
                var fileEntity = await _dbContext.Set<FileEntity>().FirstOrDefaultAsync(z => z.Id == fileId);
                if (fileEntity == null)
                {
                    return new FileDownloadResult
                    {
                        Success = false,
                        ErrorMessage = "File not found"
                    };
                }

                byte[] fileData;

                if (fileEntity.FileData != null)
                {
                    fileData = fileEntity.FileData;
                }
                else if (!string.IsNullOrEmpty(fileEntity.FilePath) && File.Exists(fileEntity.FilePath))
                {
                    fileData = await File.ReadAllBytesAsync(fileEntity.FilePath);
                }
                else
                {
                    return new FileDownloadResult
                    {
                        Success = false,
                        ErrorMessage = "file data not found"
                    };
                }

                return new FileDownloadResult
                {
                    FileData = fileData,
                    FileName = fileEntity.FileName,
                    ContentType = fileEntity.ContentType,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file; {FileId}", fileId);
                return new FileDownloadResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred while downloading the file."
                };
            }
        }

        public async Task<FileUploadResult> UploadFileAsync(IFormFile file, FileUploadOptions options)
        {
            try
            {
                if (file == null || file.Length > 0)
                    return new FileUploadResult { Success = false, ErrorMessage = "No file selected" };
                if (!ValidateFile(file, options)) return new FileUploadResult { Success = false, ErrorMessage = "File validation false" };

                var fileId = Guid.NewGuid().ToString();
                var fileName = options.GenerateUniqueFileName ? $"{fileId}_{file.FileName}" : file.FileName;

                var result = new FileUploadResult
                {
                    Success = true,
                    FileId = fileId,
                    FileName = fileName,
                    ContentType = file.ContentType,
                    UploadDate = DateTime.Now,
                    FileSize = file.Length
                };

                if (options.StorageType == StorageType.Database)
                    await SaveToDatabase(file, fileId, fileName, result);
                else
                    await SaveToFileSystem(file, fileId, fileName, result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
                throw;
            }
        }

        public async Task<List<FileUploadResult>> UploadFilesAsync(IFormFileCollection files, FileUploadOptions options)
        {
            var results = new List<FileUploadResult>();
            foreach (var file in files)
            {
                var result = await UploadFileAsync(file, options);
                results.Add(result);
            }
            return results;
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            try
            {
                var fileEntity = await _dbContext.Set<FileEntity>()
                    .FirstOrDefaultAsync(f => f.Id == fileId);

                if (fileEntity == null)
                    return false;

                // Delete from filesystem if stored there
                if (!string.IsNullOrEmpty(fileEntity.FilePath) && File.Exists(fileEntity.FilePath))
                {
                    File.Delete(fileEntity.FilePath);
                }

                // Delete from database
                _dbContext.Set<FileEntity>().Remove(fileEntity);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileId}", fileId);
                return false;
            }
        }

        private async Task SaveToDatabase(IFormFile file, string fileId, string fileName, FileUploadResult result)
        {
            using var memorySteam = new MemoryStream();
            await file.CopyToAsync(memorySteam);

            var fileEntity = new FileEntity
            {
                ContentType = file.ContentType,
                FileData = memorySteam.ToArray(),
                FileName = fileName,
                FileSize = file.Length,
                UploadDate = DateTime.Now,
                Id = fileId
            };

            _dbContext.Set<FileEntity>().Add(fileEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task SaveToFileSystem(IFormFile file, string fileStorePath, string fileName, FileUploadResult result)
        {
            if (!Directory.Exists(fileStorePath))
                Directory.CreateDirectory(fileStorePath);
            var filePath = Path.Combine(fileStorePath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var fileEntity = new FileEntity
            {
                ContentType = file.ContentType,
                FileName = fileName,
                FilePath = filePath,
                FileSize = file.Length,
                UploadDate = DateTime.Now,
                Id = result.FileId
            };

            _dbContext.Set<FileEntity>().Add(fileEntity);
            await _dbContext.SaveChangesAsync();

            result.FilePath = filePath;
        }

        private bool ValidateFile(IFormFile file, FileUploadOptions options)
        {
            if (file.Length > options.MaxFileSize)
                return false;
            if (options.AllowedExtensions != null && options.AllowedExtensions.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!options.AllowedExtensions.Contains(extension))
                    return false;
            }
            return true;
        }
       
    }
}
