using BloodLink.Domain;
using Microsoft.EntityFrameworkCore;

namespace BloodLink.Api.Data;

public class BloodLinkDbContext : DbContext
{
    public BloodLinkDbContext(DbContextOptions<BloodLinkDbContext> options) : base(options)
    {
    }

    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<BloodCenter> BloodCenters => Set<BloodCenter>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<BloodRequest> BloodRequests => Set<BloodRequest>();
    public DbSet<RequestResponse> RequestResponses => Set<RequestResponse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Donor>(e =>
        {
            e.ToTable("donors");
            e.HasKey(d => d.Id);
            e.Property(d => d.Email).IsRequired().HasMaxLength(254);
            e.HasIndex(d => d.Email).IsUnique();
            e.Property(d => d.PasswordHash).IsRequired();
            e.Property(d => d.FullName).IsRequired().HasMaxLength(100);
            e.Property(d => d.City).HasMaxLength(100);
            e.Property(d => d.Sex).HasConversion<string>().HasMaxLength(10);
            e.Property(d => d.BloodGroup).HasConversion<string>().HasMaxLength(3);
            e.Property(d => d.RhFactor).HasConversion<string>().HasMaxLength(10);
        });

        modelBuilder.Entity<BloodCenter>(e =>
        {
            e.ToTable("blood_centers");
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.Address).IsRequired().HasMaxLength(300);
            e.Property(c => c.City).IsRequired().HasMaxLength(100);
            e.Property(c => c.Phone).HasMaxLength(30);
            e.Property(c => c.WorkingHours).HasMaxLength(100);
            e.HasData(SeedData.Centers);
        });

        modelBuilder.Entity<Donation>(e =>
        {
            e.ToTable("donations");
            e.HasKey(d => d.Id);
            e.Property(d => d.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(d => d.Notes).HasMaxLength(500);
            e.HasOne(d => d.Donor)
                .WithMany(d => d.Donations)
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.BloodCenter)
                .WithMany()
                .HasForeignKey(d => d.BloodCenterId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(d => new { d.DonorId, d.Date });
        });

        modelBuilder.Entity<BloodRequest>(e =>
        {
            e.ToTable("blood_requests");
            e.HasKey(r => r.Id);
            e.Property(r => r.BloodGroup).HasConversion<string>().HasMaxLength(3);
            e.Property(r => r.RhFactor).HasConversion<string>().HasMaxLength(10);
            e.Property(r => r.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.Urgency).HasConversion<string>().HasMaxLength(10);
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(10);
            e.Property(r => r.ContactPhone).IsRequired().HasMaxLength(20);
            e.Property(r => r.PatientName).HasMaxLength(100);
            e.Property(r => r.Hospital).HasMaxLength(200);
            e.Property(r => r.Description).HasMaxLength(500);
            e.HasOne(r => r.Author)
                .WithMany()
                .HasForeignKey(r => r.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.BloodCenter)
                .WithMany()
                .HasForeignKey(r => r.BloodCenterId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(r => new { r.Status, r.BloodGroup, r.RhFactor });
        });

        modelBuilder.Entity<RequestResponse>(e =>
        {
            e.ToTable("request_responses");
            e.HasKey(r => r.Id);
            e.HasOne(r => r.Request)
                .WithMany(r => r.Responses)
                .HasForeignKey(r => r.RequestId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Donor)
                .WithMany()
                .HasForeignKey(r => r.DonorId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(r => new { r.RequestId, r.DonorId }).IsUnique();
        });
    }
}
