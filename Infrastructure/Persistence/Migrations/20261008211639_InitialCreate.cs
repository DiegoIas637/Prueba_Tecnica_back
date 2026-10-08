using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Premium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableQuota = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdvisorId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RequestReference = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    PlanCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    BuyerName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    BuyerEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Premium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsHistorical = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Code",
                table: "Plans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_AdvisorId_AcceptedAt",
                table: "Sales",
                columns: new[] { "AdvisorId", "AcceptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Sales_AdvisorId_RequestReference",
                table: "Sales",
                columns: new[] { "AdvisorId", "RequestReference" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "Sales");
        }
    }
}
