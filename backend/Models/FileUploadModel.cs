using System.ComponentModel.DataAnnotations;

namespace OpenAiChat.Models
{
    public class FileUploadModel
    {
        // [Id] INT NOT NULL PRIMARY KEY IDENTITY
        // EF Core convention automatically detects 'Id' as the primary key
        // and assumes it's an IDENTITY column (auto-incrementing) in the database.
        public int Id { get; set; }

        // [LocalFileName] NVARCHAR(90) NOT NULL
        // [Required] enforces NOT NULL constraint.
        // [MaxLength] is a good practice to match the database column size.
        [Required]
        [MaxLength(90)]
        public required string LocalFileName { get; set; }

        // [FileLengthInBytes] INT NOT NULL
        [Required]
        public int FileLengthInBytes { get; set; }

        // [AwsKey] NVARCHAR(60) NULL
        // 'string?' and no [Required] attribute allows for NULL.
        [MaxLength(60)]
        public string? AwsKey { get; set; }

        // [PresignedUrl] NVARCHAR(180) NULL
        // 'string?' and no [Required] attribute allows for NULL.
        [MaxLength(480)]
        public string? PresignedUrl { get; set; }

        // [LoadTime] DATETIMEOFFSET NULL
        // DateTimeOffset is the correct C# type for DATETIMEOFFSET in SQL Server.
        // 'DateTimeOffset?' allows for NULL.
        [Required]
        public DateTimeOffset LoadTime { get; set; }

        // [FileExtension] NVARCHAR(20) NULL
        // 'string?' and no [Required] attribute allows for NULL.
        [MaxLength(20)]
        public string? FileExtension { get; set; }
    }
}
