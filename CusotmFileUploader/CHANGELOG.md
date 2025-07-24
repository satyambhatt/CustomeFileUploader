# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - 2025-01-XX

### Added
- Initial release of FileUploadManager
- Support for single and multiple file uploads
- Configurable storage options (FileSystem/Database)
- File validation with size and extension restrictions
- Compatible with any database provider through Entity Framework Core
- Easy integration with dependency injection
- Comprehensive error handling and logging
- MVC, Web API, and Blazor support
- Complete documentation and examples

### Features
- `FileUploadAttribute` for model validation
- `IFileUploadService` for programmatic file operations
- Support for custom storage paths
- Automatic file cleanup on deletion
- Image preview functionality
- Multi-database support (SQL Server, PostgreSQL, MySQL, SQLite, etc.)