using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BikeTracking.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    private static readonly string[] columns = new[] { "AggregateId", "AggregateType" };
    private static readonly string[] columnsArray = new[] { "UserId", "CreatedTimestamp" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CommunityStatistics",
            columns: table => new
            {
                StatisticId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TotalRides = table.Column<int>(type: "int", nullable: false),
                TotalDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                AverageDistance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                RideFrequencyTrends = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LeaderboardData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CommunityStatistics", x => x.StatisticId);
            });

        migrationBuilder.CreateTable(
            name: "DataDeletionRequests",
            columns: table => new
            {
                RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                RequestTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Scope = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ProcessedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                AuditTrail = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataDeletionRequests", x => x.RequestId);
            });

        migrationBuilder.CreateTable(
            name: "Events",
            columns: table => new
            {
                EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AggregateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AggregateType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                Version = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Events", x => x.EventId);
            });

        migrationBuilder.CreateTable(
            name: "RideProjections",
            columns: table => new
            {
                RideId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Date = table.Column<DateOnly>(type: "date", nullable: false),
                Hour = table.Column<int>(type: "int", nullable: false),
                Distance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                DistanceUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RideName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                StartLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                EndLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                WeatherData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifiedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletionStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CommunityStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                AgeInDays = table.Column<int>(type: "int", nullable: false, computedColumnSql: "DATEDIFF(DAY, CAST([CreatedTimestamp] AS DATE), CAST(GETUTCDATE() AS DATE))")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RideProjections", x => x.RideId);
            });

        migrationBuilder.CreateTable(
            name: "Rides",
            columns: table => new
            {
                RideId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Date = table.Column<DateOnly>(type: "date", nullable: false),
                Hour = table.Column<int>(type: "int", nullable: false),
                Distance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                DistanceUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RideName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                StartLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                EndLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                WeatherData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifiedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletionStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CommunityStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Rides", x => x.RideId);
            });

        migrationBuilder.CreateTable(
            name: "UserPreferences",
            columns: table => new
            {
                UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                DistanceUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CommunityOptIn = table.Column<bool>(type: "bit", nullable: false),
                CreatedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                ModifiedTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserPreferences", x => x.UserId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DataDeletionRequests_UserId",
            table: "DataDeletionRequests",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Events_AggregateId_AggregateType",
            table: "Events",
            columns: columns);

        migrationBuilder.CreateIndex(
            name: "IX_RideProjections_UserId_CreatedTimestamp",
            table: "RideProjections",
            columns: columnsArray);

        migrationBuilder.CreateIndex(
            name: "IX_Rides_UserId_CreatedTimestamp",
            table: "Rides",
            columns: columnsArray);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CommunityStatistics");

        migrationBuilder.DropTable(
            name: "DataDeletionRequests");

        migrationBuilder.DropTable(
            name: "Events");

        migrationBuilder.DropTable(
            name: "RideProjections");

        migrationBuilder.DropTable(
            name: "Rides");

        migrationBuilder.DropTable(
            name: "UserPreferences");
    }
}
