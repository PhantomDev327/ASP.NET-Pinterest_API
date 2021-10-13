using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nxPinterest.Data;
using Microsoft.EntityFrameworkCore;

namespace nxPinterest.Web.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection ConfigureDatabaseOptions(this IServiceCollection service, IConfiguration config) =>
            service.AddDbContext<ApplicationDbContext>(options=> options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
    }
}
