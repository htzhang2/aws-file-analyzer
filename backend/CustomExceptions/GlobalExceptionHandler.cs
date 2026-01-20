using Amazon.S3;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace OpenAiChat.CustomExceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        async ValueTask<bool> IExceptionHandler.TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            // Customize the response based on the exception type
            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Server Error",
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                Detail = exception.Message // In production, consider hiding sensitive details
            };

            // Example of handling specific exceptions
            switch (exception)
            {
                case AmazonS3Exception:
                    var s3Ex = exception as AmazonS3Exception;

                    if (s3Ex?.StatusCode == HttpStatusCode.NotFound)
                    {
                        problemDetails.Status = (int)HttpStatusCode.BadRequest;
                        problemDetails.Title = $"AWS Bucket not exist!";
                    }
                    else
                    {
                        problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                        problemDetails.Title = $"AWS S3 error: {exception.Message}!";
                    }
                    break;

                case ArgumentNullException:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = "A required argument was null!";
                    break;
                case ArgumentException:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = "Argument was out of range!";
                    break;
                case InvalidDataException:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = "Invalid/unsupported url/contentType entered!";
                    break;
                case InvalidOperationException:
                    problemDetails.Status = (int)HttpStatusCode.TooManyRequests;
                    problemDetails.Title = "Too many requests!";
                    break;
                case UserSetupException:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = "AWS or Azure Setup problem!";
                    break;
                case AwsCloudException:
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Title = "AWS internal error!";
                    break;
                case AzureCloudException:
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Title = "Azure internal error!";
                    break;
                case OpenAiException:
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Title = "OpenAI internal error!";
                    break;
                default:
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Title = $"{exception.Message}!";
                    break;
            }

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; // Indicates the exception has been handled
        }
    }
}
