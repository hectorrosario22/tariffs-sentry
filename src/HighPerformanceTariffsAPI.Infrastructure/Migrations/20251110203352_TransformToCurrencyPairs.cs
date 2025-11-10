using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HighPerformanceTariffsAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TransformToCurrencyPairs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tariffs_RegionCode",
                table: "Tariffs");

            migrationBuilder.RenameColumn(
                name: "RegionCode",
                table: "Tariffs",
                newName: "TargetCurrency");

            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "Tariffs",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "BaseCurrency",
                table: "Tariffs",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tariffs",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_BaseCurrency",
                table: "Tariffs",
                column: "BaseCurrency");

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_BaseCurrency_IsActive",
                table: "Tariffs",
                columns: new[] { "BaseCurrency", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_EffectiveDate_IsActive",
                table: "Tariffs",
                columns: new[] { "EffectiveDate", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_IsActive",
                table: "Tariffs",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tariffs_BaseCurrency",
                table: "Tariffs");

            migrationBuilder.DropIndex(
                name: "IX_Tariffs_BaseCurrency_IsActive",
                table: "Tariffs");

            migrationBuilder.DropIndex(
                name: "IX_Tariffs_EffectiveDate_IsActive",
                table: "Tariffs");

            migrationBuilder.DropIndex(
                name: "IX_Tariffs_IsActive",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "BaseCurrency",
                table: "Tariffs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tariffs");

            migrationBuilder.RenameColumn(
                name: "TargetCurrency",
                table: "Tariffs",
                newName: "RegionCode");

            migrationBuilder.AlterColumn<decimal>(
                name: "Rate",
                table: "Tariffs",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_RegionCode",
                table: "Tariffs",
                column: "RegionCode");
        }
    }
}
