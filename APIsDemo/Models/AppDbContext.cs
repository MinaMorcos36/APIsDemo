using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace APIsDemo.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<ApplicationStatus> ApplicationStatuses { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Cvanalysis> Cvanalyses { get; set; }

    public virtual DbSet<Cvdocument> Cvdocuments { get; set; }

    public virtual DbSet<ExternalLogin> ExternalLogins { get; set; }

    public virtual DbSet<Industry> Industries { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<LikedComment> LikedComments { get; set; }

    public virtual DbSet<LikedPost> LikedPosts { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SavedPost> SavedPosts { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<SkillLevel> SkillLevels { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserSkill> UserSkills { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=APIsDemoDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Applicat__3214EC07AFDDEDF2");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Job).WithMany(p => p.Applications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__JobId__693CA210");

            entity.HasOne(d => d.Status).WithMany(p => p.Applications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__Statu__6A30C649");
        });

        modelBuilder.Entity<ApplicationStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Applicat__3214EC07739D5A44");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comment__3214EC0740635D43");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ParentComment).WithMany(p => p.InverseParentComment).HasConstraintName("FK__Comment__ParentC__71D1E811");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Comment__PostId__70DDC3D8");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Company__3214EC071A4063EC");

            entity.HasOne(d => d.Industry).WithMany(p => p.Companies)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Company__Industr__5DCAEF64");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Conversa__3214EC07AD870D84");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Analysis).WithMany(p => p.Conversations).HasConstraintName("FK__Conversat__Analy__0A9D95DB");
        });

        modelBuilder.Entity<Cvanalysis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CVAnalys__3214EC070A2134B6");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Cv).WithMany(p => p.Cvanalyses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CVAnalysis__CVId__06CD04F7");
        });

        modelBuilder.Entity<Cvdocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CVDocume__3214EC07CA7F8F8C");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<ExternalLogin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__External__3214EC07CE3F0E91");

            entity.HasOne(d => d.User).WithMany(p => p.ExternalLogins)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExternalL__UserI__45F365D3");
        });

        modelBuilder.Entity<Industry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Industry__3214EC072893618C");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Job__3214EC07646A9159");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Company).WithMany(p => p.Jobs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Job__CompanyId__628FA481");
        });

        modelBuilder.Entity<LikedComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CommentL__3214EC078EE7A75F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Comment).WithMany(p => p.LikedComments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CommentLi__Comme__00200768");
        });

        modelBuilder.Entity<LikedPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PostLike__3214EC076B099ED1");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Post).WithMany(p => p.LikedPosts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostLikes__PostI__76969D2E");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Messages__3214EC07ECA6831A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Convo).WithMany(p => p.Messages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Messages__ConvoI__0E6E26BF");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Post__3214EC07F4CC029C");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07478D85E2");

            entity.Property(e => e.ExpiresAt).HasDefaultValueSql("(dateadd(day,(7),getdate()))");
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RefreshTo__UserI__4316F928");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3214EC07DE90308F");
        });

        modelBuilder.Entity<SavedPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PostSave__3214EC076B33CD70");

            entity.Property(e => e.SavedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Post).WithMany(p => p.SavedPosts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PostSaved__PostI__7B5B524B");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Skill__3214EC07538B9E39");
        });

        modelBuilder.Entity<SkillLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SkillLev__3214EC0750687ADB");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC077FB1276A");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasOne(d => d.Role).WithMany().HasConstraintName("FK__UserRole__RoleId__3D5E1FD2");

            entity.HasOne(d => d.User).WithMany().HasConstraintName("FK__UserRole__UserId__3C69FB99");
        });

        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.SkillId }).HasName("PK__UserSkil__7A72C55493F62537");

            entity.HasOne(d => d.Level).WithMany(p => p.UserSkills)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserSkill__Level__5629CD9C");

            entity.HasOne(d => d.Skill).WithMany(p => p.UserSkills)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserSkill__Skill__5535A963");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
