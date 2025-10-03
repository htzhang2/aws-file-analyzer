using Microsoft.EntityFrameworkCore;
using OpenAiChat.Models;

namespace OpenAiChat.Data
{
    public class FileUploadEfDbContext: DbContext
    {
        public FileUploadEfDbContext(DbContextOptions<FileUploadEfDbContext> options)
        : base(options)
        {
        }

        public DbSet<FileUploadModel> FileUploadHistory { get; set; }
    }
}
