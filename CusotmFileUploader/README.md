
    # CusotmFileUploader

A comprehensive file upload manager for .NET Core applications with support for single/multiple files, database storage, file system storage, and validation attributes.

## Features

- ✅ Single and multiple file upload support
- ✅ Configurable storage options (FileSystem/Database)
- ✅ File validation with size and extension restrictions
- ✅ Compatible with any database provider through Entity Framework Core
- ✅ Easy integration with dependency injection
- ✅ Automatic file metadata management
- ✅ Built-in download and delete functionality
- ✅ Validation attributes for model binding

## Installation

```bash
dotnet add package FileUploadManager
```

## Quick Start

### 1. Configure Services

```csharp
// In Program.cs or Startup.cs
using FileUploadManager.Extensions;
using Microsoft.EntityFrameworkCore;

// Add your DbContext
builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseSqlServer(connectionString)); // or any other database provider

// Add FileUploadManager
builder.Services.AddFileUploadManager<YourDbContext>();
```

### 2. Update Your DbContext

```csharp
using FileUploadManager.Models;
using Microsoft.EntityFrameworkCore;

public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }
    
    public DbSet<FileEntity> Files { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileEntity>()
            .HasKey(f => f.Id);
    }
}
```

### 3. Create Upload Models with Attributes

```csharp
using FileUploadManager.Attributes;
using Microsoft.AspNetCore.Http;

public class DocumentUploadModel
{
    [FileUpload(
        AllowMultiple = false,
        MaxFileSize = 10 * 1024 * 1024, // 10MB
        AllowedExtensions = new[] { ".pdf", ".doc", ".docx" },
        StoragePath = "documents",
        StorageType = StorageType.FileSystem)]
    public IFormFile Document { get; set; }

    [FileUpload(
        AllowMultiple = true,
        MaxFileSize = 5 * 1024 * 1024, // 5MB
        AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" },
        StorageType = StorageType.Database)]
    public IFormFileCollection Images { get; set; }
}
```

### 4. Create Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;

    public FileController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] DocumentUploadModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var options = new FileUploadOptions
        {
            StoragePath = "uploads",
            StorageType = StorageType.FileSystem,
            MaxFileSize = 10 * 1024 * 1024,
            AllowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".png" }
        };

        var result = await _fileUploadService.UploadFileAsync(model.Document, options);
        return Ok(result);
    }

    [HttpGet("download/{fileId}")]
    public async Task<IActionResult> Download(string fileId)
    {
        var result = await _fileUploadService.DownloadFileAsync(fileId);
        
        if (!result.Success)
            return NotFound(result.ErrorMessage);

        return File(result.FileData, result.ContentType, result.FileName);
    }

    [HttpDelete("{fileId}")]
    public async Task<IActionResult> Delete(string fileId)
    {
        var success = await _fileUploadService.DeleteFileAsync(fileId);
        return success ? Ok("File deleted") : NotFound("File not found");
    }
}
```

## Configuration Options

### FileUploadAttribute Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AllowMultiple` | bool | false | Allow multiple file selection |
| `MaxFileSize` | long | 5MB | Maximum file size in bytes |
| `AllowedExtensions` | string[] | Common extensions | Allowed file extensions |
| `StoragePath` | string | "uploads" | Storage path for filesystem storage |
| `StorageType` | StorageType | FileSystem | Storage type (FileSystem/Database) |

### FileUploadOptions Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `StoragePath` | string | "uploads" | Directory path for file storage |
| `StorageType` | StorageType | FileSystem | Where to store files |
| `MaxFileSize` | long | 5MB | Maximum allowed file size |
| `AllowedExtensions` | string[] | null | Permitted file extensions |
| `GenerateUniqueFileName` | bool | true | Generate unique filenames |

## Storage Types

### FileSystem Storage
- Files stored on disk
- Metadata stored in database
- Better performance for large files
- Easier to backup and manage

```csharp
var options = new FileUploadOptions
{
    StorageType = StorageType.FileSystem,
    StoragePath = "uploads/documents"
};
```

### Database Storage
- Files stored as binary data in database
- Single location for all data
- Better for small files
- Easier deployment

```csharp
var options = new FileUploadOptions
{
    StorageType = StorageType.Database
};
```

## Database Compatibility

This package works with any database supported by Entity Framework Core:

- SQL Server
- PostgreSQL
- MySQL
- SQLite
- Oracle
- In-Memory (for testing)

Simply configure your DbContext with the appropriate provider:

```csharp
// SQL Server
builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseSqlServer(connectionString));

// PostgreSQL
builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseNpgsql(connectionString));

// MySQL
builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
```

## Advanced Usage

### Custom Validation

```csharp
[FileUpload(
    AllowMultiple = true,
    MaxFileSize = 50 * 1024 * 1024, // 50MB
    AllowedExtensions = new[] { ".mp4", ".avi", ".mkv" },
    StoragePath = "videos",
    StorageType = StorageType.FileSystem)]
[Required(ErrorMessage = "At least one video file is required")]
public IFormFileCollection VideoFiles { get; set; }
```

### Programmatic Upload

```csharp
public class FileService
{
    private readonly IFileUploadService _uploadService;

    public FileService(IFileUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    public async Task<string> SaveUserAvatar(IFormFile avatar, string userId)
    {
        var options = new FileUploadOptions
        {
            StoragePath = $"avatars/{userId}",
            StorageType = StorageType.FileSystem,
            MaxFileSize = 2 * 1024 * 1024, // 2MB
            AllowedExtensions = new[] { ".jpg", ".jpeg", ".png" },
            GenerateUniqueFileName = false
        };

        var result = await _uploadService.UploadFileAsync(avatar, options);
        return result.Success ? result.FileId : null;
    }
}
```

## Migration

Run the following command to create the database migration:

```bash
dotnet ef migrations add AddFileUploadSupport
dotnet ef database update
```

## Error Handling

The package provides comprehensive error handling:

```csharp
var result = await _fileUploadService.UploadFileAsync(file, options);

if (!result.Success)
{
    // Handle error
    Console.WriteLine($"Upload failed: {result.ErrorMessage}");
}
else
{
    // File uploaded successfully
    Console.WriteLine($"File uploaded with ID: {result.FileId}");
}
```

## License

MIT License - see LICENSE file for details.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## Support

For issues and questions, please visit the [GitHub repository](https://github.com/satyambhatt/CustomeFileUploader.git).
