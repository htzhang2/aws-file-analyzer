
namespace OpenAiChat.Services
{
    public interface IImageService
    {
        Task<string> AnalyzeImageAsync(string imageUrl);
    }
}