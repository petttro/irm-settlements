using System;
using System.Collections.Generic;
using IRM.Settlements.Domain.Entities;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IRM.Settlements.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "settlements");

            migrationBuilder.CreateTable(
                name: "Mvz",
                schema: "settlements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    MvzCode = table.Column<string>(type: "text", nullable: true),
                    MvzName = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mvz", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                schema: "settlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<string>(type: "text", nullable: false),
                    ServiceDateFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ServiceDateTo = table.Column<DateOnly>(type: "date", nullable: false),
                    ServiceCompanySapId = table.Column<string>(type: "text", nullable: false),
                    ServiceCompanyName = table.Column<string>(type: "text", nullable: false),
                    ServiceCenterExternalIds = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    SentToPaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric", nullable: false),
                    ItemsCount = table.Column<int>(type: "integer", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCenters",
                schema: "settlements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServiceCompanySapId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCenters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCompanies",
                schema: "settlements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SapId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportItems",
                schema: "settlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CouponNumber = table.Column<string>(type: "text", nullable: false),
                    SaleOrderNumber = table.Column<string>(type: "text", nullable: true),
                    OrderNumber = table.Column<string>(type: "text", nullable: true),
                    ServiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServiceName = table.Column<string>(type: "text", nullable: false),
                    ServiceCompanySapId = table.Column<string>(type: "text", nullable: false),
                    ServiceCenterExternalId = table.Column<string>(type: "text", nullable: false),
                    ServiceCenterName = table.Column<string>(type: "text", nullable: false),
                    CityKisId = table.Column<string>(type: "text", nullable: false),
                    CityName = table.Column<string>(type: "text", nullable: false),
                    ShopName = table.Column<string>(type: "text", nullable: false),
                    WareCode = table.Column<string>(type: "text", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchText = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SapPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    SapNds = table.Column<string>(type: "text", nullable: true),
                    CheckNumber = table.Column<int>(type: "integer", nullable: false),
                    ConfirmType = table.Column<int>(type: "integer", nullable: true),
                    BsiStatus = table.Column<int>(type: "integer", nullable: false),
                    AdditionalServices = table.Column<List<AdditionalService>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportItems_Mvz_ShopName",
                        column: x => x.ShopName,
                        principalSchema: "settlements",
                        principalTable: "Mvz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportItems_Reports_ReportId",
                        column: x => x.ReportId,
                        principalSchema: "settlements",
                        principalTable: "Reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appeals",
                schema: "settlements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CouponNumber = table.Column<string>(type: "text", nullable: false),
                    SaleOrderNumber = table.Column<string>(type: "text", nullable: true),
                    OrderNumber = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    WareCode = table.Column<string>(type: "text", nullable: false),
                    ServiceName = table.Column<string>(type: "text", nullable: false),
                    ServicePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    CityKisId = table.Column<string>(type: "text", nullable: false),
                    CityName = table.Column<string>(type: "text", nullable: false),
                    ServiceCompanyId = table.Column<int>(type: "integer", nullable: false),
                    ServiceCenterId = table.Column<int>(type: "integer", nullable: false),
                    ShopName = table.Column<string>(type: "text", nullable: false),
                    AdditionalServices = table.Column<List<AdditionalService>>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    BsiStatus = table.Column<int>(type: "integer", nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    ConfirmType = table.Column<int>(type: "integer", nullable: true),
                    ProjectTypeId = table.Column<int>(type: "integer", nullable: false),
                    ReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appeals_ServiceCenters_ServiceCenterId",
                        column: x => x.ServiceCenterId,
                        principalSchema: "settlements",
                        principalTable: "ServiceCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appeals_ServiceCompanies_ServiceCompanyId",
                        column: x => x.ServiceCompanyId,
                        principalSchema: "settlements",
                        principalTable: "ServiceCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appeals_CouponNumber",
                schema: "settlements",
                table: "Appeals",
                column: "CouponNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appeals_ServiceCenterId",
                schema: "settlements",
                table: "Appeals",
                column: "ServiceCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Appeals_ServiceCompanyId",
                schema: "settlements",
                table: "Appeals",
                column: "ServiceCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportItems_CouponNumber",
                schema: "settlements",
                table: "ReportItems",
                column: "CouponNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportItems_ReportId_ServiceDate",
                schema: "settlements",
                table: "ReportItems",
                columns: new[] { "ReportId", "ServiceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportItems_ShopName",
                schema: "settlements",
                table: "ReportItems",
                column: "ShopName");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ServiceCompanySapId_CreatedAt",
                schema: "settlements",
                table: "Reports",
                columns: new[] { "ServiceCompanySapId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCenters_ExternalId",
                schema: "settlements",
                table: "ServiceCenters",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCenters_ServiceCompanySapId",
                schema: "settlements",
                table: "ServiceCenters",
                column: "ServiceCompanySapId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCompanies_SapId",
                schema: "settlements",
                table: "ServiceCompanies",
                column: "SapId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appeals",
                schema: "settlements");

            migrationBuilder.DropTable(
                name: "ReportItems",
                schema: "settlements");

            migrationBuilder.DropTable(
                name: "ServiceCenters",
                schema: "settlements");

            migrationBuilder.DropTable(
                name: "ServiceCompanies",
                schema: "settlements");

            migrationBuilder.DropTable(
                name: "Mvz",
                schema: "settlements");

            migrationBuilder.DropTable(
                name: "Reports",
                schema: "settlements");
        }
    }
}
