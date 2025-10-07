using OpenAiChat.Models;

namespace OpenAiChat.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<FileUploadModel> FileUploadHistory { get; }

        IGenericRepository<FileAnalysisResultModel> FileAnalysisResult { get; }

        Task<bool> IsDbConnectionStringGood();
        Task<int> CompleteAsync();
    }
}
