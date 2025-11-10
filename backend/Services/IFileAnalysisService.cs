
namespace OpenAiChat.Services
{
    public interface IFileAnalysisService
    {
        Task<string> AnalyzeFileAsync(string fileUrl);
    }
}