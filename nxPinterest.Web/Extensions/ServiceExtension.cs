using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nxPinterest.Data;

namespace nxPinterest.Web.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection ConfigureDatabaseOptions(this IServiceCollection service, IConfiguration config) =>
            service.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
    }
}
