namespace BikeTracking.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using BikeTracking.Domain.Entities;
using BikeTracking.Domain.ValueObjects;
using System.Text.Json;

public class BikeTrackingContext : DbContext
{
    public BikeTrackingContext(DbContextOptions<BikeTrackingContext> options)
        : base(options)
    {
    }

    public DbSet<Ride> Rides { get; set; } = null!;
    public DbSet<UserPreference> UserPreferences { get; set; } = null!;
    public DbSet<DataDeletionRequest> DataDeletionRequests { get; set; } = null!;
    public DbSet<CommunityStatistics> CommunityStatistics { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<RideProjection> RideProjections { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Event Store
        modelBuilder.Entity<Event>()
            .HasKey(e => e.EventId);
        modelBuilder.Entity<Event>()
            .HasIndex(e => new { e.AggregateId, e.AggregateType });
        modelBuilder.Entity<Event>()
            .Property(e => e.EventData)
            .HasColumnType("nvarchar(max)");

        // Rides
        modelBuilder.Entity<Ride>()
            .HasKey(r => r.RideId);
        modelBuilder.Entity<Ride>()
            .Property(r => r.RideName)
            .HasMaxLength(200)
            .IsRequired();
        modelBuilder.Entity<Ride>()
            .Property(r => r.StartLocation)
            .HasMaxLength(200)
            .IsRequired();
        modelBuilder.Entity<Ride>()
            .Property(r => r.EndLocation)
            .HasMaxLength(200)
            .IsRequired();
        modelBuilder.Entity<Ride>()
            .Property(r => r.WeatherData)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<Weather>(v, (JsonSerializerOptions?)null))
            .HasColumnType("nvarchar(max)");
        modelBuilder.Entity<Ride>()
            .HasIndex(r => new { r.UserId, r.CreatedTimestamp });

        // RideProjections
        modelBuilder.Entity<RideProjection>()
            .HasKey(rp => rp.RideId);
        modelBuilder.Entity<RideProjection>()
            .Property(rp => rp.RideName)
            .HasMaxLength(200);
        modelBuilder.Entity<RideProjection>()
            .Property(rp => rp.StartLocation)
            .HasMaxLength(200);
        modelBuilder.Entity<RideProjection>()
            .Property(rp => rp.EndLocation)
            .HasMaxLength(200);
        modelBuilder.Entity<RideProjection>()
            .Property(rp => rp.WeatherData)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<Weather>(v, (JsonSerializerOptions?)null))
            .HasColumnType("nvarchar(max)");
        modelBuilder.Entity<RideProjection>()
            .HasIndex(rp => new { rp.UserId, rp.CreatedTimestamp });
        modelBuilder.Entity<RideProjection>()
            .Property(rp => rp.AgeInDays)
            .HasComputedColumnSql("DATEDIFF(DAY, CAST([CreatedTimestamp] AS DATE), CAST(GETUTCDATE() AS DATE))")
            .ValueGeneratedOnAddOrUpdate();

        // UserPreferences
        modelBuilder.Entity<UserPreference>()
            .HasKey(up => up.UserId);

        // DataDeletionRequests
        modelBuilder.Entity<DataDeletionRequest>()
            .HasKey(ddr => ddr.RequestId);
        modelBuilder.Entity<DataDeletionRequest>()
            .HasIndex(ddr => ddr.UserId);

        // CommunityStatistics
        modelBuilder.Entity<CommunityStatistics>()
            .HasKey(cs => cs.StatisticId);
        modelBuilder.Entity<CommunityStatistics>()
            .Property(cs => cs.RideFrequencyTrends)
            .HasColumnType("nvarchar(max)");
        modelBuilder.Entity<CommunityStatistics>()
            .Property(cs => cs.LeaderboardData)
            .HasColumnType("nvarchar(max)");
    }
}
