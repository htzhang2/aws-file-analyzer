using OpenAiChat.Models;
using OpenAiChat.Repository;
using OpenAiChat.Utils;

namespace OpenAiChat.Services
{
    public class FileAnalysisService : IFileAnalysisService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FileAnalysisService> _logger;
        private readonly IImageService _imageService;
        private readonly ITextService _textService;
        private readonly IPdfService _pdfService;
        private readonly IUnitOfWork _unitOfWork;

        public FileAnalysisService(
            IHttpClientFactory httpClientFactory,
            ILogger<FileAnalysisService> logger,
            IImageService imageService,
            ITextService textService,
            IPdfService pdfService,
            IUnitOfWork unitOfWork)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _imageService = imageService;
            _textService = textService;
            _pdfService = pdfService;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> AnalyzeFileAsync(string fileUrl)
        {
            if (!FileUtils.IsFileUrlValid(fileUrl))
            {
                throw new InvalidDataException("Invalid/unsupported url entered!");
            }

            // 1. Download HTML
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");

            // Check file type
            var typeRequest = new HttpRequestMessage(HttpMethod.Get, fileUrl);
            var headerResponse = await httpClient.SendAsync(typeRequest);

            if (headerResponse == null || !headerResponse.IsSuccessStatusCode)
            {
                throw new InvalidDataException("Unable to fetch header!");
            }

            string contentType = headerResponse.Content.Headers.ContentType.MediaType;

            if (string.IsNullOrEmpty(contentType))
            {
                throw new InvalidDataException("Content formet of {textUrl} NOT supported!");
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

                    return geoInfo;
                }
                catch (Exception ex)
                {
                    // something wrong with server
                    throw new Exception(ex.Message);
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

                    return summary;
                }
                catch (Exception ex)
                {
                    // TODO: custom TooManyRequestsException
                    throw new InvalidOperationException(ex.Message);
                }
            }
            else if (FileUtils.IsPdfFile(contentType))
            {
                try
                {
                    // Ask the service to summarize
                    var summary = await _pdfService.SummarizePdfAsync(fileUrl).ConfigureAwait(false);

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

                    return summary;
                }
                catch (Exception ex)
                {
                    // TODO: custom TooManyRequestsException
                    throw new InvalidOperationException(ex.Message);
                }
            }
            else
            {
                throw new InvalidDataException("Unsupported link content");
            }
        }
    }
}
