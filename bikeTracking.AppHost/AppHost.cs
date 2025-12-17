var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");
// SQL Server Integration Reference:
// https://aspire.dev/integrations/databases/sql-server/#add-sql-server-resource-and-database-resource

//var sqlPassword = builder.AddParameter("sql-password", new SqlPasswordDefault());    // password can be overridden in user secrets or environment variables

// the EF migrations are not ran automatically, dotnet ef database update --connection "Server=127.0.0.1,1433;User ID=sa;Password=;Initial Catalog=BikeTrackingDb

#pragma warning disable ASPIREPROXYENDPOINTS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

var sql = builder.AddSqlServer("sql")
                .WithImageTag("2025-latest")
                .WithContainerName("biketracking-sql")
                .WithDataVolume("biketracking-sql-data")
                .WithLifetime(ContainerLifetime.Persistent)
                .WithEndpointProxySupport(false);
#pragma warning restore ASPIREPROXYENDPOINTS001

var db = sql.AddDatabase("BikeTrackingDb");
var apiService = builder.AddProject<Projects.bikeTracking_ApiService>("apiservice")
    .WithReference(db)
    .WaitFor(db)
    .WithReference(cache)
    .WaitFor(cache);

builder.AddProject<Projects.bikeTracking_WebWasm>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
