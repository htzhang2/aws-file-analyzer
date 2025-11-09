using Amazon.Runtime.Internal;
using static System.Net.WebRequestMethods;

namespace OpenAiChat.Utils
{
    public class FileUtils
    {
        private static readonly HashSet<string> ImageContentTypes = new HashSet<string>()
        {
            "image/png",
            "image/jpeg",
            "image/gif"
        };
        private static readonly HashSet<string> PlainTextContentTypes = new HashSet<string>()
        {
            "text/html",
            "text/plain"
        };
        private static readonly string PdfContentType = "application/pdf";

        public static bool IsImage(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }
            return ImageContentTypes.Contains(contentType);
        }

        public static bool IsPlainText(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }
            return PlainTextContentTypes.Contains(contentType);
        }

        public static bool IsPdfFile(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }

            // TODO: check header bytes for PDF magic number %PDF-
            return PdfContentType.Equals(contentType);
        }

        /// <summary>
        ///  check if file url is valid
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <returns>true for valid http/https url; false otherwise</returns>
        public static bool IsFileUrlValid(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
            {
                return false;
            }

            // A Uri object to hold the result if successful
            Uri uriResult;

            // Use UriKind.Absolute to ensure it's a fully qualified URI (e.g., https://example.com)
            // Use UriKind.RelativeOrAbsolute to allow relative paths (e.g., /api/resource)
            // We'll use Absolute for most external checks.

            bool isValid = Uri.TryCreate(fileUrl, UriKind.Absolute, out uriResult);

            if (!isValid)
            {
                return false;
            }

            if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
            {
                // Only http/ https supported
                return false;
            }

            return true;
        }
    }
}
