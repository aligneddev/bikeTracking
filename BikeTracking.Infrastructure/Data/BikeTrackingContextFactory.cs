using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BikeTracking.Infrastructure.Data;

public class BikeTrackingContextFactory : IDesignTimeDbContextFactory<BikeTrackingContext>
{
    public BikeTrackingContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BikeTrackingContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BikeTracking;Trusted_Connection=True;");
        
        return new BikeTrackingContext(optionsBuilder.Options);
    }
}
