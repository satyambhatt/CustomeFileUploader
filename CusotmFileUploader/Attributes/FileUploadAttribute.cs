using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CusotmFileUploader.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FileUploadAttribute : ValidationAttribute
    {
        public bool AllowMultiple { get; set; } = false;
        public long MaxFileSize { get; set; } = 5 * 1024 * 1024; // 5MB default
        public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };
        public string StoragePath { get; set; } = "uploads";
        public StorageType StorageType { get; set; } = StorageType.FileSystem;
        public bool StoreInDatabase { get; set; } = false;
        public override bool IsValid(object value)
        {
            if (value == null) return true; // Let [Required] handle null validation

            if (value is IFormFile file)
            {
                return ValidateFile(file);
            }
            else if (value is IFormFileCollection files)
            {
                if (!AllowMultiple && files.Count > 1)
                {
                    ErrorMessage = "Multiple files are not allowed for this field.";
                    return false;
                }

                return files.All(ValidateFile);
            }

            return false;
        }

        private bool ValidateFile(IFormFile file)
        {
            // Check file size
            if (file.Length > MaxFileSize)
            {
                ErrorMessage = $"File size cannot exceed {MaxFileSize / (1024 * 1024)}MB.";
                return false;
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (AllowedExtensions != null && AllowedExtensions.Length > 0 &&
                !AllowedExtensions.Contains(extension))
            {
                ErrorMessage = $"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", AllowedExtensions)}.";
                return false;
            }

            return true;
        }
    }
    public enum StorageType
    {
        FileSystem,
        Database
    }
}
