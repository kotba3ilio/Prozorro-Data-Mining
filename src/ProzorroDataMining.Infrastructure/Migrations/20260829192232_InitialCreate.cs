using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProzorroDataMining.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "import_sync_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeedName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BackwardNextPageUri = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ForwardStartPageUri = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ForwardNextPageUri = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    LastDirection = table.Column<int>(type: "integer", nullable: false),
                    LastPublicModified = table.Column<decimal>(type: "numeric(20,6)", precision: 20, scale: 6, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSuccessAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_sync_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    IdentifierScheme = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    IdentifierId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tender_import_jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClassificationId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MaxPages = table.Column<int>(type: "integer", nullable: false),
                    PageSize = table.Column<int>(type: "integer", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResultDirection = table.Column<int>(type: "integer", nullable: true),
                    FeedItemsScanned = table.Column<int>(type: "integer", nullable: true),
                    CandidatesFound = table.Column<int>(type: "integer", nullable: true),
                    ImportedCount = table.Column<int>(type: "integer", nullable: true),
                    UpdatedCount = table.Column<int>(type: "integer", nullable: true),
                    SkippedCount = table.Column<int>(type: "integer", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: true),
                    NextPageUri = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    PrevPageUri = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tender_import_jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProzorroId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProcuringEntityName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExpectedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    ImportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tender_contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProzorroContractId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AwardId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: ""),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    DateSigned = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tender_contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tender_contracts_tenders_TenderId",
                        column: x => x.TenderId,
                        principalTable: "tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tender_import_payloads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenderId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProzorroId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PublicModified = table.Column<decimal>(type: "numeric", nullable: true),
                    SourceDateModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    PayloadHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ImportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tender_import_payloads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tender_import_payloads_tenders_TenderId",
                        column: x => x.TenderId,
                        principalTable: "tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tender_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassificationId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tender_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tender_items_tenders_TenderId",
                        column: x => x.TenderId,
                        principalTable: "tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tender_suppliers",
                columns: table => new
                {
                    TenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwardId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tender_suppliers", x => new { x.TenderId, x.SupplierId, x.AwardId });
                    table.ForeignKey(
                        name: "FK_tender_suppliers_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tender_suppliers_tenders_TenderId",
                        column: x => x.TenderId,
                        principalTable: "tenders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_import_sync_states_FeedName",
                table: "import_sync_states",
                column: "FeedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_IdentifierScheme_IdentifierId",
                table: "suppliers",
                columns: new[] { "IdentifierScheme", "IdentifierId" });

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_Name",
                table: "suppliers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_tender_contracts_DateSigned",
                table: "tender_contracts",
                column: "DateSigned");

            migrationBuilder.CreateIndex(
                name: "IX_tender_contracts_ProzorroContractId",
                table: "tender_contracts",
                column: "ProzorroContractId");

            migrationBuilder.CreateIndex(
                name: "IX_tender_contracts_TenderId",
                table: "tender_contracts",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_tender_contracts_TenderId_AwardId",
                table: "tender_contracts",
                columns: new[] { "TenderId", "AwardId" });

            migrationBuilder.CreateIndex(
                name: "IX_tender_import_jobs_CreatedAt",
                table: "tender_import_jobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_tender_import_jobs_Status",
                table: "tender_import_jobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tender_import_payloads_ImportedAt",
                table: "tender_import_payloads",
                column: "ImportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_tender_import_payloads_ProzorroId",
                table: "tender_import_payloads",
                column: "ProzorroId");

            migrationBuilder.CreateIndex(
                name: "IX_tender_import_payloads_PublicModified",
                table: "tender_import_payloads",
                column: "PublicModified");

            migrationBuilder.CreateIndex(
                name: "IX_tender_import_payloads_TenderId",
                table: "tender_import_payloads",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_tender_items_ClassificationId",
                table: "tender_items",
                column: "ClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_tender_items_ClassificationId_TenderId",
                table: "tender_items",
                columns: new[] { "ClassificationId", "TenderId" });

            migrationBuilder.CreateIndex(
                name: "IX_tender_items_TenderId",
                table: "tender_items",
                column: "TenderId");

            migrationBuilder.CreateIndex(
                name: "IX_tender_suppliers_AwardId",
                table: "tender_suppliers",
                column: "AwardId");

            migrationBuilder.CreateIndex(
                name: "IX_tender_suppliers_SupplierId",
                table: "tender_suppliers",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_tenders_DateCreated",
                table: "tenders",
                column: "DateCreated");

            migrationBuilder.CreateIndex(
                name: "IX_tenders_ImportedAt",
                table: "tenders",
                column: "ImportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_tenders_ProcuringEntityName",
                table: "tenders",
                column: "ProcuringEntityName");

            migrationBuilder.CreateIndex(
                name: "IX_tenders_ProzorroId",
                table: "tenders",
                column: "ProzorroId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenders_Status",
                table: "tenders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "import_sync_states");

            migrationBuilder.DropTable(
                name: "tender_contracts");

            migrationBuilder.DropTable(
                name: "tender_import_jobs");

            migrationBuilder.DropTable(
                name: "tender_import_payloads");

            migrationBuilder.DropTable(
                name: "tender_items");

            migrationBuilder.DropTable(
                name: "tender_suppliers");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "tenders");
        }
    }
}
