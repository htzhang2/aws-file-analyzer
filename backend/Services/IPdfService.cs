
namespace OpenAiChat.Services
{
    public interface IPdfService
    {
        Task<string> SummarizePdfAsync(string pdfUrl);
    }
}