using Amazon.S3;
using Amazon.S3.Model;
using OpenAiChat.CustomExceptions;
using OpenAiChat.Models;
using OpenAiChat.Repository;

namespace OpenAiChat.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly ILogger<FileAnalysisService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IAmazonS3 _s3Client;

        public FileUploadService(
            IAmazonS3 s3Client,
            ILogger<FileAnalysisService> logger,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _s3Client = s3Client;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            string bucketName = _configuration["AWS:S3BucketName"];
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new UserSetupException("Empty bucket name");
            }

            bool isConnectionStringGood = await _unitOfWork.IsDbConnectionStringGood().ConfigureAwait(false);

            if (isConnectionStringGood)
            {
                // Check file already loaded before
                var existingFile = _unitOfWork.FileUploadHistory.Find(f => (
                    f.LocalFileName == file.FileName &&
                    f.FileLengthInBytes == file.Length)).FirstOrDefault();

                if (existingFile != null)
                {
                    var loadDate = existingFile.LoadTime.ToString("d");
                    throw new UserSetupException($"File already loaded on {loadDate}!");
                }
            }

            var key = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); // Use a unique key

            try
            {
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    InputStream = file.OpenReadStream(),
                    ContentType = file.ContentType
                };

                await _s3Client.PutObjectAsync(putObjectRequest);

                var presignedUrl = await GeneratePreSignedUrl(key, 60, bucketName);

                // Save file meta data to SQL if connection string is valid
                if (isConnectionStringGood)
                {
                    // prepare file meta information
                    var model = new FileUploadModel
                    {
                        LocalFileName = file.FileName,
                        FileLengthInBytes = (int)(file.Length),
                        AwsKey = key,
                        PresignedUrl = presignedUrl,
                        FileExtension = file.ContentType,
                        LoadTime = DateTime.Now
                    };


                    _unitOfWork.FileUploadHistory.Add(model);
                    await _unitOfWork.CompleteAsync().ConfigureAwait(false);
                }

                return presignedUrl;
            }
            catch (AmazonS3Exception ex)
            {
                throw new AwsCloudException($"S3 error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new AwsCloudException($"Internal server error: {ex.Message}");
            }
        }

        private async Task<string> GeneratePreSignedUrl(string objectKey, double durationInMinutes, string bucketName)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddMinutes(durationInMinutes),
                Verb = HttpVerb.GET // Use HttpVerb.PUT for uploads
            };

            string preSignedUrl = await _s3Client.GetPreSignedURLAsync(request);

            return preSignedUrl;
        }
    }
}
