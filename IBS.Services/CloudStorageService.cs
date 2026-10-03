using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using IBS.Utility;
using IBS.Utility.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IBS.Services
{
    public interface ICloudStorageService
    {
        Task<string> GetSignedUrlAsync(string fileNameToRead, int timeOutInMinutes = 30);

        Task<string> UploadFileAsync(IFormFile fileToUpload, string fileNameToSave);

        Task DeleteFileAsync(string fileNameToDelete);

        Task<Stream> DownloadFileAsync(string fileNameToDownload);

        Task<IFormFile?> GetFileAsFormFile(string fileName);
    }

    public class CloudStorageService : ICloudStorageService
    {
        private const string _localFilesDirectoryName = "files";
        private readonly GCSConfigOptions _options;
        private readonly ILogger<CloudStorageService> _logger;
        private readonly IHostEnvironment _environment;
        private readonly GoogleCredential _googleCredential = null!;
        private readonly StorageClient _storageClient = null!;

        public CloudStorageService(
            IOptions<GCSConfigOptions> options,
            ILogger<CloudStorageService> logger,
            IHostEnvironment environment)
        {
            _options = options.Value;
            _logger = logger;
            _environment = environment;

            if (_environment.IsDevelopment())
            {
                return;
            }

            try
            {
                var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (environmentName == Environments.Production)
                {
                    _googleCredential = GoogleCredential.GetApplicationDefault();
                }
                else
                {
                    // Log for debugging purposes
                    _logger.LogInformation($"Environment: {environmentName}, Auth File: {_options.GCPStorageAuthFile}");

                    if (!File.Exists(_options.GCPStorageAuthFile))
                    {
                        throw new FileNotFoundException($"Auth file not found: {_options.GCPStorageAuthFile}");
                    }

                    using var stream = File.OpenRead(_options.GCPStorageAuthFile);

                    var serviceAccountCredential = CredentialFactory.FromStream<ServiceAccountCredential>(stream);

                    _googleCredential = serviceAccountCredential.ToGoogleCredential();
                }

                _storageClient = StorageClient.Create(_googleCredential);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize Google Cloud Storage client: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteFileAsync(string fileNameToDelete)
        {
            if (_environment.IsDevelopment())
            {
                File.Delete(GetLocalFilePath(fileNameToDelete));
                return;
            }

            try
            {
                await _storageClient.DeleteObjectAsync(_options.GoogleCloudStorageBucketName, fileNameToDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting file: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetSignedUrlAsync(string fileNameToRead, int timeOutInMinutes = 30)
        {
            if (_environment.IsDevelopment())
            {
                var fileName = GetLocalFileName(fileNameToRead);
                return $"/{_localFilesDirectoryName}/{Uri.EscapeDataString(fileName)}";
            }

            try
            {
                var bucketName = _options.GoogleCloudStorageBucketName;
                var urlSigner = UrlSigner.FromCredential(_googleCredential);

                var signedUrl = await urlSigner.SignAsync(bucketName, fileNameToRead, TimeSpan.FromMinutes(timeOutInMinutes));

                _logger.LogInformation($"Signed URL obtained for file '{fileNameToRead}'");
                return signedUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while obtaining signed URL for file: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UploadFileAsync(IFormFile fileToUpload, string fileNameToSave)
        {
            if (fileToUpload == null || fileToUpload.Length == 0)
            {
                _logger.LogError("File upload failed: No file provided or file is empty.");
                throw new ArgumentException("File is either null or empty.", nameof(fileToUpload));
            }

            if (_environment.IsDevelopment())
            {
                if (!string.Equals(fileNameToSave, Path.GetFileName(fileNameToSave), StringComparison.Ordinal))
                {
                    throw new ArgumentException("File name must not contain a path.", nameof(fileNameToSave));
                }

                var filesDirectory = Path.Combine(_environment.ContentRootPath, "wwwroot", _localFilesDirectoryName);
                Directory.CreateDirectory(filesDirectory);

                var filePath = GetLocalFilePath(fileNameToSave);
                await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await fileToUpload.CopyToAsync(fileStream);

                return Path.Combine(_localFilesDirectoryName, fileNameToSave).Replace('\\', '/');
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await fileToUpload.CopyToAsync(memoryStream);
                    memoryStream.Position = 0; // Reset stream position after copying

                    var uploadedFile = await _storageClient.UploadObjectAsync(
                        _options.GoogleCloudStorageBucketName,
                        fileNameToSave,
                        fileToUpload.ContentType ?? "application/octet-stream",
                        memoryStream
                    );
                    return uploadedFile.MediaLink;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while uploading file: {ex.Message}");
                throw;
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileNameToDownload)
        {
            if (_environment.IsDevelopment())
            {
                return new FileStream(GetLocalFilePath(fileNameToDownload), FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            try
            {
                using (var storageClient = StorageClient.Create(_googleCredential))
                {
                    var memoryStream = new MemoryStream();
                    await storageClient.DownloadObjectAsync(_options.GoogleCloudStorageBucketName, fileNameToDownload, memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin); // Reset stream position to the beginning for reading
                    _logger.LogInformation($"File {fileNameToDownload} downloaded successfully");
                    return memoryStream;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while downloading file: {ex.Message}");
                throw;
            }
        }

        private string GetLocalFilePath(string fileName)
        {
            return Path.Combine(_environment.ContentRootPath, "wwwroot", _localFilesDirectoryName, GetLocalFileName(fileName));
        }

        private static string GetLocalFileName(string fileName)
        {
            var normalizedPath = fileName.Replace('\\', '/').TrimStart('/');
            var localDirectoryPrefix = $"{_localFilesDirectoryName}/";

            if (normalizedPath.StartsWith(localDirectoryPrefix, StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = normalizedPath[localDirectoryPrefix.Length..];
            }

            if (string.IsNullOrWhiteSpace(normalizedPath)
                || !string.Equals(normalizedPath, Path.GetFileName(normalizedPath), StringComparison.Ordinal))
            {
                throw new ArgumentException("File name must not contain a path.", nameof(fileName));
            }

            return normalizedPath;
        }

        public async Task<IFormFile?> GetFileAsFormFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("File name is required.");
            }

            try
            {
                var fileStream = await DownloadFileAsync(fileName);

                if (fileStream == null || fileStream.Length == 0)
                {
                    throw new FileNotFoundException("File not found.", fileName);
                }

                var formFile = new FormFile(fileStream, 0, fileStream.Length, "file", fileName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/octet-stream",
                };

                return formFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing file: {ex.Message}");
                throw;
            }
        }
    }
}
