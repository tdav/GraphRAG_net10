using Microsoft.EntityFrameworkCore;
using GraphRAG.Domain.Entities.Core;
using GraphRAG.Domain.Entities.Medical;
using GraphRAG.Domain.Entities.Graph;
using GraphRAG.Domain.Entities.AI;

namespace GraphRAG.Infrastructure.Data;

public class PostgresDbContext : DbContext
{
    public PostgresDbContext(DbContextOptions<PostgresDbContext> options) 
        : base(options)
    {
    }

    // Core entities
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Conversation> Conversations { get; set; } = null!;

    // Medical entities
    public DbSet<Patient> Patients { get; set; } = null!;
    public DbSet<Condition> Conditions { get; set; } = null!;
    public DbSet<MedicationRequest> MedicationRequests { get; set; } = null!;
    public DbSet<Observation> Observations { get; set; } = null!;

    // Graph entities
    public DbSet<GraphNode> GraphNodes { get; set; } = null!;
    public DbSet<GraphEdge> GraphEdges { get; set; } = null!;
    public DbSet<Concept> Concepts { get; set; } = null!;

    // AI/ML entities
    public DbSet<Embedding> Embeddings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema
        modelBuilder.HasDefaultSchema("graphrag");

        // Configure pgvector extension
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.HasPostgresExtension("age");

        // Configure entities
        ConfigureTenants(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureConversations(modelBuilder);
        ConfigurePatients(modelBuilder);
        ConfigureConditions(modelBuilder);
        ConfigureMedicationRequests(modelBuilder);
        ConfigureObservations(modelBuilder);
        ConfigureGraphNodes(modelBuilder);
        ConfigureGraphEdges(modelBuilder);
        ConfigureConcepts(modelBuilder);
        ConfigureEmbeddings(modelBuilder);
    }

    private static void ConfigureTenants(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.HasOne<Tenant>()
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureConversations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("conversations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Tenant>()
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePatients(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("patients");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FhirId).HasMaxLength(100);
            entity.HasIndex(e => new { e.TenantId, e.FhirId }).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.HasOne<Tenant>()
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureConditions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Condition>(entity =>
        {
            entity.ToTable("conditions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FhirId).HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CodeSystem).HasMaxLength(200);
            entity.Property(e => e.Display).HasMaxLength(500);
            entity.Property(e => e.ClinicalStatus).HasMaxLength(50);
            entity.HasOne<Patient>()
                  .WithMany()
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Tenant>()
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureMedicationRequests(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MedicationRequest>(entity =>
        {
            entity.ToTable("medication_requests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FhirId).HasMaxLength(100);
            entity.Property(e => e.MedicationCode).HasMaxLength(50);
            entity.Property(e => e.CodeSystem).HasMaxLength(200);
            entity.Property(e => e.MedicationDisplay).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne<Patient>()
                  .WithMany()
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Tenant>()
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureObservations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Observation>(entity =>
        {
            entity.ToTable("observations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FhirId).HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CodeSystem).HasMaxLength(200);
            entity.Property(e => e.Display).HasMaxLength(500);
            entity.Property(e => e.Value).HasMaxLength(500);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne<Patient>()
                  .WithMany()
                  .HasForeignKey(e => e.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Tenant>()
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureGraphNodes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GraphNode>(entity =>
        {
            entity.ToTable("graph_nodes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Label).IsRequired().HasMaxLength(100);
            entity.Property(e => e.GraphName).HasMaxLength(100);
            entity.HasOne<Tenant>()
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureGraphEdges(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GraphEdge>(entity =>
        {
            entity.ToTable("graph_edges");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EdgeType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.GraphName).HasMaxLength(100);
            entity.HasOne<GraphNode>()
                  .WithMany()
                  .HasForeignKey(e => e.SourceNodeId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<GraphNode>()
                  .WithMany()
                  .HasForeignKey(e => e.TargetNodeId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Tenant>()
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureConcepts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Concept>(entity =>
        {
            entity.ToTable("concepts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.System).HasMaxLength(100);
            entity.Property(e => e.Display).HasMaxLength(500);
            entity.HasIndex(e => new { e.System, e.Code });
            entity.HasOne<Tenant>()
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureEmbeddings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Embedding>(entity =>
        {
            entity.ToTable("embeddings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.Model).HasMaxLength(100);
            // pgvector column - will be handled by migration
            entity.HasOne<Tenant>()
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
