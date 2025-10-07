using OpenAiChat.Data;
using OpenAiChat.Models;
using OpenAiChat.Utils;

namespace OpenAiChat.Repository
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly FileUploadEfDbContext _context;

        public UnitOfWork(FileUploadEfDbContext context)
        {
            _context = context;
            FileUploadHistory = new GenericRepository<FileUploadModel>(_context);
            FileAnalysisResult = new GenericRepository<FileAnalysisResultModel>(_context);
        }

        public async Task<bool> IsDbConnectionStringGood()
        {
            bool isConnectionStringGood = await DbUtils.IsDbConnectionStringGood(_context).ConfigureAwait(false);

            return isConnectionStringGood;
        }
        public IGenericRepository<FileUploadModel> FileUploadHistory { get; private set; }
        public IGenericRepository<FileAnalysisResultModel> FileAnalysisResult { get; private set; }

        public async Task<int> CompleteAsync()
        {
            var savedCount = await _context.SaveChangesAsync().ConfigureAwait(false);
            return savedCount;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
