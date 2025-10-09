using System.ComponentModel.DataAnnotations;

namespace OpenAiChat.Models
{
    public class UserLoginModel
    {
        // [Id] INT NOT NULL PRIMARY KEY IDENTITY
        // EF Core convention automatically detects 'Id' as the primary key
        // and assumes it's an IDENTITY column (auto-incrementing) in the database.
        public int Id { get; set; }

        // [Username] NVARCHAR(100) NOT NULL
        // [Required] enforces NOT NULL constraint.
        // [MaxLength] is a good practice to match the database column size.
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        // [Password] NVARCHAR(Max) NOT NULL
        [Required]
        public string Password { get; set; } = string.Empty;

    }
}
