using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphRAG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "graphrag");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:age", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "graphrag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Configuration = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "concepts",
                schema: "graphrag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    System = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Display = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Definition = table.Column<string>(type: "text", nullable: true),
                    ParentConceptsJson = table.Column<string>(type: "text", nullable: true),
                    EmbeddingId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_concepts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_concepts_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "graphrag",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "embeddings",
                schema: "graphrag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Vector = table.Column<float[]>(type: "real[]", nullable: true),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_embeddings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_embeddings_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "graphrag",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "graph_nodes",
                schema: "graphrag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PropertiesJson = table.Column<string>(type: "text", nullable: false),
                    GraphName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AgeVertexId = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_graph_nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_graph_nodes_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "graphrag",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "patients",
                schema: "graphrag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FhirId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FhirDataJson = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patients_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "graphrag",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "graphrag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "graphrag",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "graph_edges",
                schema: "graphrag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EdgeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PropertiesJson = table.Column<string>(type: "text", nullable: false),
                    GraphName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AgeEdgeId = table.Column<long>(type: "bigint", nullable: true),
                    Weight = table.Column<double>(type: "double precision", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_graph_edges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_graph_edges_graph_nodes_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalSchema: "graphrag",
                        principalTable: "graph_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_graph_edges_graph_nodes_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalSchema: "graphrag",
                        principalTable: "graph_nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_graph_edges_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "graphrag",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "conditions",
                schema: "graphrag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FhirId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CodeSystem = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Display = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ClinicalStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OnsetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FhirDataJson = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conditions_patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "graphrag",
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_conditions_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "graphrag",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medication_requests",
                schema: "graphrag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FhirId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CodeSystem = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MedicationDisplay = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DosageInstructions = table.Column<string>(type: "text", nullable: true),
                    AuthoredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FhirDataJson = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medication_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_medication_requests_patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "graphrag",
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_medication_requests_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "graphrag",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "observations",
                schema: "graphrag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FhirId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CodeSystem = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Display = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EffectiveDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FhirDataJson = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_observations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_observations_patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "graphrag",
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_observations_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "graphrag",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                schema: "graphrag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MessagesJson = table.Column<string>(type: "text", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conversations_tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "graphrag",
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_conversations_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "graphrag",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_concepts_System_Code",
                schema: "graphrag",
                table: "concepts",
                columns: new[] { "System", "Code" });

            migrationBuilder.CreateIndex(
                name: "IX_concepts_TenantId",
                schema: "graphrag",
                table: "concepts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_conditions_PatientId",
                schema: "graphrag",
                table: "conditions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_conditions_TenantId",
                schema: "graphrag",
                table: "conditions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_TenantId",
                schema: "graphrag",
                table: "conversations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_UserId",
                schema: "graphrag",
                table: "conversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_embeddings_TenantId",
                schema: "graphrag",
                table: "embeddings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_graph_edges_SourceNodeId",
                schema: "graphrag",
                table: "graph_edges",
                column: "SourceNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_graph_edges_TargetNodeId",
                schema: "graphrag",
                table: "graph_edges",
                column: "TargetNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_graph_edges_TenantId",
                schema: "graphrag",
                table: "graph_edges",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_graph_nodes_TenantId",
                schema: "graphrag",
                table: "graph_nodes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_medication_requests_PatientId",
                schema: "graphrag",
                table: "medication_requests",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_medication_requests_TenantId",
                schema: "graphrag",
                table: "medication_requests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_observations_PatientId",
                schema: "graphrag",
                table: "observations",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_observations_TenantId",
                schema: "graphrag",
                table: "observations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_patients_TenantId_FhirId",
                schema: "graphrag",
                table: "patients",
                columns: new[] { "TenantId", "FhirId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Name",
                schema: "graphrag",
                table: "tenants",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_TenantId_Email",
                schema: "graphrag",
                table: "users",
                columns: new[] { "TenantId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "concepts",
                schema: "graphrag");

            migrationBuilder.DropTable(
                name: "conditions",
                schema: "graphrag");

            migrationBuilder.DropTable(
                name: "conversations",
                schema: "graphrag");

            migrationBuilder.DropTable(
                name: "embeddings",
                schema: "graphrag");

            migrationBuilder.DropTable(
                name: "graph_edges",
                schema: "graphrag");

            migrationBuilder.DropTable(
                name: "medication_requests",
                schema: "graphrag");

            migrationBuilder.DropTable(
                name: "observations",
                schema: "graphrag");

            migrationBuilder.DropTable(
                name: "users",
                schema: "graphrag");

            migrationBuilder.DropTable(
                name: "graph_nodes",
                schema: "graphrag");

            migrationBuilder.DropTable(
                name: "patients",
                schema: "graphrag");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "graphrag");
        }
    }
}
