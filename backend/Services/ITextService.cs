
namespace OpenAiChat.Services
{
    public interface ITextService
    {
        Task<string> SummarizeTextAsync(string textUrl);
    }
}