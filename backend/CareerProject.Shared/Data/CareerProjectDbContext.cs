using CareerProject.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerProject.Shared.Data;

public class CareerProjectDbContext : DbContext
{
    public CareerProjectDbContext(DbContextOptions<CareerProjectDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<PersonProfile> PersonProfiles => Set<PersonProfile>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<PersonSkill> PersonSkills => Set<PersonSkill>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobSkill> JobSkills => Set<JobSkill>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var vectorColumnType = $"vector({EmbeddingDefaults.Dimensions})";

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasOne(u => u.PersonProfile)
                .WithOne(p => p.User)
                .HasForeignKey<PersonProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(u => u.Company)
                .WithOne(c => c.User)
                .HasForeignKey<Company>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PersonProfile>(entity =>
        {
            entity.Property(p => p.Embedding).HasColumnType(vectorColumnType);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasIndex(s => s.Name).IsUnique();
            entity.HasData(SkillSeedData.Skills);
        });

        modelBuilder.Entity<PersonSkill>(entity =>
        {
            entity.HasKey(ps => new { ps.PersonId, ps.SkillId });

            entity.HasOne(ps => ps.Person)
                .WithMany(p => p.PersonSkills)
                .HasForeignKey(ps => ps.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ps => ps.Skill)
                .WithMany(s => s.PersonSkills)
                .HasForeignKey(ps => ps.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.Property(j => j.Embedding).HasColumnType(vectorColumnType);

            entity.HasOne(j => j.Company)
                .WithMany(c => c.Jobs)
                .HasForeignKey(j => j.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobSkill>(entity =>
        {
            entity.HasKey(js => new { js.JobId, js.SkillId });

            entity.HasOne(js => js.Job)
                .WithMany(j => j.JobSkills)
                .HasForeignKey(js => js.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(js => js.Skill)
                .WithMany(s => s.JobSkills)
                .HasForeignKey(js => js.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasIndex(a => new { a.JobId, a.PersonId }).IsUnique();

            entity.HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Person)
                .WithMany(p => p.Applications)
                .HasForeignKey(a => a.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasOne(m => m.Person)
                .WithMany(p => p.ChatMessages)
                .HasForeignKey(m => m.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(n => n.RecipientUser)
                .WithMany()
                .HasForeignKey(n => n.RecipientUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            // One Application can never have more than one Conversation.
            entity.HasIndex(c => c.ApplicationId).IsUnique();

            entity.HasOne(c => c.Application)
                .WithMany()
                .HasForeignKey(c => c.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DirectMessage>(entity =>
        {
            entity.HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
