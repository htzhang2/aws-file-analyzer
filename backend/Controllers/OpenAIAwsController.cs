using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using OpenAiChat.Dto;
using OpenAiChat.Models;
using OpenAiChat.Repository;
using OpenAiChat.Services;
using OpenAiChat.Utils;
using System.Net;

namespace OpenAiChat.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class OpenAIAwsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OpenAIAwsController> _logger;
        private readonly ChatClient _chatClient;
        private readonly IAmazonS3 _s3Client;
        private readonly IImageService _imageService;
        private readonly ITextService _textService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public OpenAIAwsController(
            IHttpClientFactory httpClientFactory,
            IAmazonS3 s3Client,
            ChatClient chatClient,
            IImageService imageService,
            ITextService textService,
            ILogger<OpenAIAwsController> logger,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _s3Client = s3Client;
            _chatClient = chatClient;
            _textService = textService;
            _imageService = imageService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        /// <summary>
        ///  List file name and presigned url under s3 bucket name
        /// </summary>
        /// <returns></returns>
        /// [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))] // Success response map (fileName/presigned url)
        /// [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400: wrong bucket
        /// [ProducesResponseType(StatusCodes.Status500InternalServerError)] // 500: internal server error
        [HttpGet("ListS3Files")]
        public async Task<IActionResult> GetS3FilesUrls()
        {
            string bucketName = _configuration["AWS:S3BucketName"];

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                return BadRequest("Empty bucket name");
            }

            try
            {
                var files = new Dictionary<string, string>();
                var request = new ListObjectsV2Request
                {
                    BucketName = bucketName
                };

                ListObjectsV2Response response;
                do
                {
                    response = await _s3Client.ListObjectsV2Async(request);
                    if (response != null)
                    {
                        if (response.S3Objects != null)
                        {
                            foreach (var s3Object in response.S3Objects)
                            {
                                var preSignedUrl = await GeneratePreSignedUrl(s3Object.Key, 60, bucketName);

                                files[s3Object.Key] = preSignedUrl;
                            }
                        }

                        request.ContinuationToken = response.NextContinuationToken;
                    }
                    
                } while (response != null && response.IsTruncated != null ? (bool)(response.IsTruncated) : false);

                return Ok(files);

            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return BadRequest($"Bucket {bucketName} not exist!");
                }
                return StatusCode(500, $"S3 error: {ex.Message}");
            }
        }

        /// <summary>
        ///  List file loaded last few days
        /// </summary>
        /// <param name="days">how many days ahead</param>
        /// <param name="filesLimit">how many files to display</param>
        /// <returns>Status code of each file load record</returns>
        /// [ProducesResponseType(StatusCodes.Status200OK // Success response file list
        /// [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400: invalid input or wrong connection string
        /// [ProducesResponseType(StatusCodes.Status404NotFound)] // 404: no files loaded
        /// [ProducesResponseType(StatusCodes.Status500InternalServerError)] // 500: internal server error
        [HttpGet("ListLoadHistory")]
        public async Task<IActionResult> GetLoadHistory([FromQuery]int days=1, [FromQuery]int filesLimit=30)
        {
            if (days <= 0 || filesLimit <= 0)
            {
                return BadRequest($"Invalid input!");
            }

            var now = DateTimeOffset.UtcNow;
            var fromDate = now.AddDays(-days);
            
            // Test connection string
            bool isConnectionStringGood = await _unitOfWork.IsDbConnectionStringGood().ConfigureAwait(false);

            if (!isConnectionStringGood)
            {
                return BadRequest($"Connection string is wrong!");
            }

            try
            {
                // Only take several files
                var result = await _unitOfWork.FileUploadHistory
                    .Find(f => f.LoadTime >= fromDate && f.LoadTime <= now)
                    .Take(filesLimit)
                    .ToListAsync()
                    .ConfigureAwait(false);

                if (result != null && result.Any())
                {
                    return Ok(result);
                }

                return NotFound($"No files loaded last {days}");
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"EF error: {ex.Message}");
            }
        }
        /// <summary>
        ///  List all analyzed files
        /// </summary>
        /// <returns>Status code</returns>
        /// [ProducesResponseType(StatusCodes.Status200OK // Success response parsed files
        /// [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400: wrong connection string
        /// [ProducesResponseType(StatusCodes.Status404NotFound)] // 404: no files loaded
        /// [ProducesResponseType(StatusCodes.Status500InternalServerError)] // 500: internal server error
        [HttpGet("ListAnalysisResults")]
        public async Task<IActionResult> GetAnalysisResults()
        {
            // Test connection string
            bool isConnectionStringGood = await _unitOfWork.IsDbConnectionStringGood().ConfigureAwait(false);

            if (!isConnectionStringGood)
            {
                return BadRequest($"Connection string is wrong!");
            }

            try
            {
                var query = from a in _unitOfWork.FileUploadHistory.GetDbSet()
                            join b in _unitOfWork.FileAnalysisResult.GetDbSet()
                            on a.PresignedUrl equals b.PresignedUrl
                            select new
                            {
                                a.LocalFileName,
                                a.FileExtension,
                                b.AnalysisText
                            };
                var results = await query.ToListAsync()
                    .ConfigureAwait(false);

                if (results != null && results.Any())
                {
                    return Ok(results);
                }

                return NotFound($"No files analyzed");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"EF error: {ex.Message}");
            }
        }

        /// <summary>
        ///  Upload a file to AWS S3
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Status code of upload status with presigned url</returns>
        /// [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))] // Success response string (presigned url)
        /// [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400: empty file
        /// [ProducesResponseType(StatusCodes.Status500InternalServerError)] // 500: internal server error
        [HttpPost("AwsFileUpload")]
        public async Task<IActionResult> FileUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty or not provided.");
            }

            string bucketName = _configuration["AWS:S3BucketName"];
            if (string.IsNullOrWhiteSpace(bucketName))
            {
                return BadRequest("Empty bucket name");
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

                bool isConnectionStringGood = await _unitOfWork.IsDbConnectionStringGood().ConfigureAwait(false);

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
                    
                return Ok(new { fileUrl = presignedUrl });
            }
            catch (AmazonS3Exception ex)
            {
                return StatusCode(500, $"S3 error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        ///  Analyze image url geolcation information or text url summary
        /// </summary>
        /// <param name="request">Request with fileUrl in http or https format</param>
        /// <returns>status code</returns>
        /// [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))] // Success response string (geolocation/summary)
        /// [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400: Invalid request url
        /// [ProducesResponseType(StatusCodes.Status500InternalServerError)] // 500: internal server error
        [HttpPost("OpenAISummary")]
        public async Task<IActionResult> SummarizeFile([FromBody] OpenAISummaryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.fileUrl))
            {
                return BadRequest("No request url entered!");
            }

            string fileUrl = request.fileUrl;
            
            if (!FileUtils.IsFileUrlValid(fileUrl))
            {
                return BadRequest("Invalid/unsupported url entered!");
            }

            // 1. Download HTML
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");
            
            // Check file type
            var typeRequest = new HttpRequestMessage(HttpMethod.Get, fileUrl);
            var headerResponse = await httpClient.SendAsync(typeRequest);

            if (headerResponse == null || !headerResponse.IsSuccessStatusCode)
            {
                return BadRequest("Unable to fetch header!");
            }

            string contentType = headerResponse.Content.Headers.ContentType.MediaType;

            if (string.IsNullOrEmpty(contentType))
            {
                return BadRequest("Content formet of {textUrl} NOT supported!");
            }

            // Find image info
            if (FileUtils.IsImage(contentType))
            {
                try
                {
                    var geoInfo = await _imageService.AnalyzeImageAsync(fileUrl).ConfigureAwait(false);

                    bool isConnectionStringGood = await _unitOfWork.IsDbConnectionStringGood().ConfigureAwait(false);

                    if (isConnectionStringGood)
                    {
                        var analysisResult = new FileAnalysisResultModel()
                        {
                            PresignedUrl = fileUrl,
                            AnalysisText = geoInfo,
                        };

                        _unitOfWork.FileAnalysisResult.Add(analysisResult);
                        var savedCount = await _unitOfWork.CompleteAsync().ConfigureAwait(false);
                    }

                    return Ok(geoInfo);
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                         new { message = ex.Message });
                }

            }
            // Summarize text
            else if (FileUtils.IsPlainText(contentType))
            {
                try
                {
                    // Ask the service to summarize
                    var summary = await _textService.SummarizeTextAsync(fileUrl).ConfigureAwait(false);

                    bool isConnectionStringGood = await _unitOfWork.IsDbConnectionStringGood().ConfigureAwait(false);

                    if (isConnectionStringGood)
                    {
                        var analysisResult = new FileAnalysisResultModel()
                        {
                            PresignedUrl = fileUrl,
                            AnalysisText = summary,
                        };

                        _unitOfWork.FileAnalysisResult.Add(analysisResult);
                        var savedCount = await _unitOfWork.CompleteAsync().ConfigureAwait(false);
                    }

                    return Ok(summary);
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status429TooManyRequests,
                         new { message = ex.Message });
                }
            }
            else
            {
                return BadRequest("Unsupported link content");
            }


        }

        /// <summary>
        ///  Complete prompt
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        /// [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))] // Success reply string
        /// [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400: empty prompt
        /// [ProducesResponseType(StatusCodes.Status429TooManyRequests)] // 429: too many requests
        [HttpPost("OpenAIChat")]
        public async Task<IActionResult> CompleteChat([FromBody] string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return BadRequest("No prompt entered!");
            }

            try
            {
                ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt).ConfigureAwait(false);
                return Ok(completion.Content[0].Text);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                     new { message = ex.Message });
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
