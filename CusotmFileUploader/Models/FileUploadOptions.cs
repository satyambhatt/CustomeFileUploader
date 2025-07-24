using CusotmFileUploader.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusotmFileUploader.Models
{
    public class FileUploadOptions
    {
        public string StoragePath { get; set; } = "uploads";
        public StorageType StorageType { get; set; } = StorageType.FileSystem;
        public long MaxFileSize { get; set; } = 5 * 1024 * 1024;
        public string[] AllowedExtensions { get; set; }
        public bool GenerateUniqueFileName { get; set; } = true;
    }
}
