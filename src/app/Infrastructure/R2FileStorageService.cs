using AbbaFleet.Shared;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace AbbaFleet.Infrastructure;

public class R2FileStorageService : IFileStorageService, IDisposable
{
    private readonly Lazy<AmazonS3Client> _clientLazy;
    private readonly string _bucketName;
    private readonly ILogger<R2FileStorageService> _logger;

    public R2FileStorageService(IConfiguration configuration, ILogger<R2FileStorageService> logger)
    {
        _logger = logger;
        _bucketName = configuration["Supabase:BucketName"] ?? string.Empty;

        var endpoint = configuration["Supabase:S3Endpoint"] ?? string.Empty;
        var accessKeyId = configuration["Supabase:AccessKeyId"] ?? string.Empty;
        var secretAccessKey = configuration["Supabase:SecretAccessKey"] ?? string.Empty;

        _clientLazy = new Lazy<AmazonS3Client>(() =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = endpoint,
                ForcePathStyle = true
            };
            return new AmazonS3Client(new BasicAWSCredentials(accessKeyId, secretAccessKey), config);
        });
    }

    private AmazonS3Client Client => _clientLazy.Value;

    public async Task<string> SaveAsync(Stream stream, string fileName, NoteEntityType entityType, Guid entityId)
    {
        var sanitized = SanitizeFileName(fileName);
        var key = $"{entityType}/{entityId}/{Guid.NewGuid()}_{sanitized}";

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            AutoCloseStream = false
        };

        await Client.PutObjectAsync(request);

        _logger.LogInformation("Saved file to R2: {Key}", key);

        return key;
    }

    public async Task DeleteAsync(string storagePath)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = storagePath
        };

        await Client.DeleteObjectAsync(request);

        _logger.LogInformation("Deleted file from R2: {Key}", storagePath);
    }

    public async Task<Stream?> OpenReadAsync(string storagePath)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = storagePath
            };

            var response = await Client.GetObjectAsync(request);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            _logger.LogWarning("R2 object not found: {Key}", storagePath);
            return null;
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(fileName.Select(c => invalid.Contains(c) ? '_' : c));
    }

    public void Dispose()
    {
        if (_clientLazy.IsValueCreated)
        {
            _clientLazy.Value.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
