var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var sql = builder.AddSqlServer("sql")
                 .AddDatabase("BikeTrackingDb");
var apiService = builder.AddProject<Projects.bikeTracking_ApiService>("apiservice")
    .WithReference(sql)
    .WithReference(cache)
    .WaitFor(cache);

builder.AddProject<Projects.bikeTracking_WebWasm>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
