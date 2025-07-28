using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusotmFileUploader.Models
{
    public class FileEntity
    {
        [Key]
        public string Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public byte[]? FileData { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
