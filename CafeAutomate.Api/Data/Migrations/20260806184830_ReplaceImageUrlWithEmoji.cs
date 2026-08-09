using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeAutomate.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceImageUrlWithEmoji : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "AllMenuItems",
                newName: "Emoji");

            // Existing rows hold image URLs in this column; replace them with
            // the default emoji so they render correctly.
            migrationBuilder.Sql(
                "UPDATE \"AllMenuItems\" SET \"Emoji\" = '☕' WHERE \"Emoji\" IS NULL OR \"Emoji\" = '' OR length(\"Emoji\") > 8;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Emoji",
                table: "AllMenuItems",
                newName: "ImageUrl");
        }
    }
}
