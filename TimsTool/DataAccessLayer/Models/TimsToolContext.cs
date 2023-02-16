using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Models;

public partial class TimsToolContext : DbContext
{
    public TimsToolContext()
    {
    }

    public TimsToolContext(int timeoutSeconds)
    {
        base.Database.SetCommandTimeout(timeoutSeconds);
    }

    public TimsToolContext(DbContextOptions<TimsToolContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<FixedList> FixedLists { get; set; }

    public virtual DbSet<Now> Nows { get; set; }

    public virtual DbSet<NowRequirement> NowRequirements { get; set; }

    public virtual DbSet<NowSubscription> NowSubscriptions { get; set; }

    public virtual DbSet<ResultDefinition> ResultDefinitions { get; set; }

    public virtual DbSet<ResultDefinitionRule> ResultDefinitionRules { get; set; }

    public virtual DbSet<ResultDefinitionWordGroup> ResultDefinitionWordGroups { get; set; }

    public virtual DbSet<ResultPrompt> ResultPrompts { get; set; }

    public virtual DbSet<ResultPromptRule> ResultPromptRules { get; set; }

    public virtual DbSet<ResultPromptWordGroup> ResultPromptWordGroups { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //generate using following in PM Console
        //Scaffold-DbContext "Data Source=.;Initial Catalog=TimsTool;Integrated Security=True; TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir..\DataAccessLayer\Models -f -namespace DataAccessLayer.Models

#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=TimsTool;Integrated Security=True; TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Comment");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MasterUuid).HasColumnName("MasterUUID");
            entity.Property(e => e.Note).IsUnicode(false);
            entity.Property(e => e.ParentMasterUuid).HasColumnName("ParentMasterUUID");
            entity.Property(e => e.ParentUuid).HasColumnName("ParentUUID");
            entity.Property(e => e.PublicationTags)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatusDate).HasColumnType("datetime");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SystemCommentType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Uuid).HasColumnName("UUID");
        });

        modelBuilder.Entity<FixedList>(entity =>
        {
            entity.HasKey(e => e.Uuid);

            entity.ToTable("FixedList");

            entity.Property(e => e.Uuid)
                .ValueGeneratedNever()
                .HasColumnName("UUID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MasterUuid).HasColumnName("MasterUUID");
            entity.Property(e => e.Payload).IsUnicode(false);
            entity.Property(e => e.PublicationTags)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatusDate).HasColumnType("datetime");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<Now>(entity =>
        {
            entity.HasKey(e => e.Uuid);

            entity.ToTable("Now");

            entity.Property(e => e.Uuid)
                .ValueGeneratedNever()
                .HasColumnName("UUID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MasterUuid).HasColumnName("MasterUUID");
            entity.Property(e => e.Payload).IsUnicode(false);
            entity.Property(e => e.PublicationTags)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatusDate).HasColumnType("datetime");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<NowRequirement>(entity =>
        {
            entity.HasKey(e => e.Uuid);

            entity.ToTable("NowRequirement");

            entity.Property(e => e.Uuid)
                .ValueGeneratedNever()
                .HasColumnName("UUID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MasterUuid).HasColumnName("MasterUUID");
            entity.Property(e => e.Nowuuid).HasColumnName("NOWUUID");
            entity.Property(e => e.ParentNowRequirementUuid).HasColumnName("ParentNowRequirementUUID");
            entity.Property(e => e.Payload).IsUnicode(false);
            entity.Property(e => e.PublicationTags)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatusDate).HasColumnType("datetime");
            entity.Property(e => e.ResultDefinitionUuid).HasColumnName("ResultDefinitionUUID");
            entity.Property(e => e.RootParentNowRequirementUuid).HasColumnName("RootParentNowRequirementUUID");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<NowSubscription>(entity =>
        {
            entity.HasKey(e => e.Uuid);

            entity.ToTable("NowSubscription");

            entity.Property(e => e.Uuid)
                .ValueGeneratedNever()
                .HasColumnName("UUID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MasterUuid).HasColumnName("MasterUUID");
            entity.Property(e => e.Payload).IsUnicode(false);
            entity.Property(e => e.PublicationTags)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatusDate).HasColumnType("datetime");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<ResultDefinition>(entity =>
        {
            entity.HasKey(e => e.Uuid);

            entity.ToTable("ResultDefinition");

            entity.Property(e => e.Uuid)
                .ValueGeneratedNever()
                .HasColumnName("UUID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MasterUuid).HasColumnName("MasterUUID");
            entity.Property(e => e.Payload).IsUnicode(false);
            entity.Property(e => e.PublicationTags)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatusDate).HasColumnType("datetime");
            entity.Property(e => e.ResultDefinitionWordGroupIds)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<ResultDefinitionRule>(entity =>
        {
            entity.HasKey(e => e.Uuid);

            entity.ToTable("ResultDefinitionRule");

            entity.Property(e => e.Uuid)
                .ValueGeneratedNever()
                .HasColumnName("UUID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MasterUuid).HasColumnName("MasterUUID");
            entity.Property(e => e.PublicationTags)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatusDate).HasColumnType("datetime");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Rule)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ResultDefinitionWordGroup>(entity =>
        {
            entity.HasKey(e => e.Uuid);

            entity.ToTable("ResultDefinitionWordGroup");

            entity.Property(e => e.Uuid)
                .ValueGeneratedNever()
                .HasColumnName("UUID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MasterUuid).HasColumnName("MasterUUID");
            entity.Property(e => e.Payload).IsUnicode(false);
            entity.Property(e => e.PublicationTags)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatusDate).HasColumnType("datetime");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<ResultPrompt>(entity =>
        {
            entity.HasKey(e => e.Uuid);

            entity.ToTable("ResultPrompt");

            entity.Property(e => e.Uuid)
                .ValueGeneratedNever()
                .HasColumnName("UUID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MasterUuid).HasColumnName("MasterUUID");
            entity.Property(e => e.Payload).IsUnicode(false);
            entity.Property(e => e.PublicationTags)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatusDate).HasColumnType("datetime");
            entity.Property(e => e.ResultPromptWordGroupIds)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        modelBuilder.Entity<ResultPromptRule>(entity =>
        {
            entity.HasKey(e => e.Uuid);

            entity.ToTable("ResultPromptRule");

            entity.Property(e => e.Uuid)
                .ValueGeneratedNever()
                .HasColumnName("UUID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MasterUuid).HasColumnName("MasterUUID");
            entity.Property(e => e.PublicationTags)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatusDate).HasColumnType("datetime");
            entity.Property(e => e.ResultDefinitionUuid).HasColumnName("ResultDefinitionUUID");
            entity.Property(e => e.ResultPromptUuid).HasColumnName("ResultPromptUUID");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Rule)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UserGroups)
                .HasMaxLength(1000)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ResultPromptWordGroup>(entity =>
        {
            entity.HasKey(e => e.Uuid);

            entity.ToTable("ResultPromptWordGroup");

            entity.Property(e => e.Uuid)
                .ValueGeneratedNever()
                .HasColumnName("UUID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.LastModifiedUser)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.MasterUuid).HasColumnName("MasterUUID");
            entity.Property(e => e.Payload).IsUnicode(false);
            entity.Property(e => e.PublicationTags)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PublishedStatusDate).HasColumnType("datetime");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
