using System.ComponentModel.DataAnnotations;

namespace OpenAiChat.Models
{
    public class FileAnalysisResultModel
    {
        // [Id] INT NOT NULL PRIMARY KEY IDENTITY
        // EF Core convention automatically detects 'Id' as the primary key
        // and assumes it's an IDENTITY column (auto-incrementing) in the database.
        public int Id { get; set; }

        // [PresignedUrl] NVARCHAR(180) NULL
        // 'string?' and no [Required] attribute allows for NULL.
        [MaxLength(480)]
        [Required]
        public string PresignedUrl { get; set; }

        // [AnalysisText] NVARCHAR(MAX) NULL
        // 'string?' and no [Required] attribute allows for NULL.
        public string? AnalysisText { get; set; }
    }
}
