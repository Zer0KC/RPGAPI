using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RpgApi.Migrations
{
    /// <inheritdoc />
    public partial class MigracaoDisputa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_DISPUTAS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Dt_Disputa = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AtacanteId = table.Column<int>(type: "int", nullable: false),
                    OponenteId = table.Column<int>(type: "int", nullable: false),
                    Tx_Narracao = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_DISPUTAS", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "TB_USUARIOS",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 120, 201, 126, 197, 214, 108, 69, 113, 99, 112, 1, 12, 23, 216, 173, 190, 251, 251, 140, 96, 126, 189, 169, 233, 64, 226, 201, 175, 121, 160, 110, 78, 78, 206, 175, 35, 232, 150, 149, 208, 249, 156, 98, 165, 218, 221, 221, 199, 226, 84, 233, 83, 160, 194, 74, 126, 72, 32, 189, 157, 32, 153, 11, 180 }, new byte[] { 231, 3, 195, 81, 185, 205, 55, 238, 54, 166, 69, 241, 105, 14, 100, 79, 248, 123, 159, 177, 49, 127, 226, 49, 163, 129, 156, 100, 63, 71, 80, 42, 247, 240, 211, 99, 92, 65, 26, 149, 24, 218, 163, 47, 245, 110, 76, 213, 34, 38, 78, 36, 71, 19, 85, 54, 164, 105, 116, 226, 68, 153, 60, 163, 161, 48, 74, 207, 233, 142, 72, 153, 7, 56, 215, 221, 213, 164, 226, 24, 244, 70, 15, 126, 160, 72, 131, 36, 203, 163, 251, 49, 192, 31, 73, 229, 181, 69, 57, 130, 129, 11, 29, 38, 43, 5, 18, 116, 175, 195, 41, 64, 206, 252, 251, 135, 47, 252, 87, 237, 252, 226, 15, 224, 154, 30, 193, 21 } });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_DISPUTAS");

            migrationBuilder.UpdateData(
                table: "TB_USUARIOS",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 39, 96, 75, 37, 191, 131, 180, 133, 38, 20, 137, 103, 250, 51, 218, 94, 32, 146, 123, 151, 105, 137, 154, 93, 119, 52, 12, 252, 100, 2, 64, 11, 206, 52, 104, 180, 127, 43, 87, 60, 233, 241, 224, 95, 193, 60, 208, 93, 9, 22, 29, 192, 40, 110, 9, 14, 89, 97, 85, 219, 133, 129, 252, 198 }, new byte[] { 129, 215, 199, 157, 227, 30, 61, 117, 207, 233, 180, 110, 156, 166, 10, 130, 201, 64, 45, 191, 82, 194, 248, 147, 154, 211, 152, 79, 251, 179, 248, 99, 228, 232, 126, 222, 227, 196, 229, 47, 123, 236, 78, 150, 166, 248, 100, 143, 254, 120, 181, 104, 181, 57, 209, 219, 254, 199, 192, 152, 65, 66, 134, 53, 86, 72, 127, 181, 22, 147, 225, 23, 22, 34, 252, 58, 186, 177, 184, 140, 103, 166, 102, 231, 15, 22, 254, 209, 39, 236, 74, 16, 25, 85, 30, 42, 162, 46, 99, 240, 178, 93, 161, 93, 176, 249, 217, 71, 87, 26, 185, 186, 55, 15, 23, 239, 76, 5, 105, 65, 221, 66, 109, 142, 85, 213, 28, 1 } });
        }
    }
}
