using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenAiChat.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileUploadHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocalFileName = table.Column<string>(type: "nvarchar(90)", maxLength: 90, nullable: false),
                    FileLengthInBytes = table.Column<int>(type: "int", nullable: false),
                    AwsKey = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    PresignedUrl = table.Column<string>(type: "nvarchar(480)", maxLength: 480, nullable: true),
                    LoadTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FileExtension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUploadHistory", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileUploadHistory");
        }
    }
}
