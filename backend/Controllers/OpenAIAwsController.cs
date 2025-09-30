using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using OpenAiChat.Dto;
using OpenAiChat.Services;
using OpenAiChat.Utils;

namespace OpenAiChat.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OpenAIAwsController : ControllerBase
    {
        private const string S3BucketName = "[TO_BE_UPDATE]";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OpenAIAwsController> _logger;
        private readonly ChatClient _chatClient;
        private readonly IAmazonS3 _s3Client;
        private readonly IImageService _imageService;
        private readonly ITextService _textService;

        public OpenAIAwsController(
            IHttpClientFactory httpClientFactory,
            IAmazonS3 s3Client,
            ChatClient chatClient,
            IImageService imageService,
            ITextService textService,
            ILogger<OpenAIAwsController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _s3Client = s3Client;
            _chatClient = chatClient;
            _textService = textService;
            _imageService = imageService;
            _logger = logger;
        }

        /// <summary>
        ///  List file name and presigned url under s3 bucket name
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        /// [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))] // Success response map (fileName/presigned url)
        /// [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400: wrong bucket
        /// [ProducesResponseType(StatusCodes.Status500InternalServerError)] // 500: internal server error
        [HttpGet("ListS3Files")]
        public async Task<IActionResult> GetS3FilesUrls([FromQuery] string bucketName = S3BucketName)
        {
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
                                var preSignedUrl = await GeneratePreSignedUrl(s3Object.Key, 60);

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
                return StatusCode(500, $"S3 error: {ex.Message}");
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

            var key = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); // Use a unique key

            try
            {
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = S3BucketName,
                    Key = key,
                    InputStream = file.OpenReadStream(),
                    ContentType = file.ContentType
                };

                await _s3Client.PutObjectAsync(putObjectRequest);

                var presignedUrl = await GeneratePreSignedUrl(key, 60);

                // var fileUrl = $"https://{S3BucketName}.s3.amazonaws.com/{key}";

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
                    var result = await _textService.SummarizeTextAsync(fileUrl).ConfigureAwait(false);
                    
                    return Ok(result);
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

        private async Task<string> GeneratePreSignedUrl(string objectKey, double durationInMinutes, string bucketName = S3BucketName)
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
