using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nxPinterest.Data;
using nxPinterest.Data.Repositories;
using nxPinterest.Data.Repositories.Interfaces;
using nxPinterest.Services;
using nxPinterest.Web.Models;

namespace nxPinterest.Web.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection ConfigureDatabaseOptions(this IServiceCollection service, IConfiguration config) =>
            service.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));


        public static IServiceCollection ConfigureOptions(this IServiceCollection service, IConfiguration config) {

            service.Configure<DestinationPathModel>(config.GetSection("DestinationPath"));

            return service;
        }

        public static IServiceCollection AddServices(this IServiceCollection service) {
            service.AddTransient<Services.Interfaces.IUserMediaManagementService, Services.UserMediaManagementService>();
            service.AddTransient<Services.Interfaces.IUserContainerManagementService, Services.UserContainerManagementService>();
            service.AddTransient<Services.Interfaces.IUserAlbumService, Services.UserAlbumService>();
            service.AddTransient<Services.Interfaces.IUserAlbumMediaService, Services.UserAlbumMediaService>();
            // repository
            service.AddTransient(typeof(IUnitOfWork), typeof(UnitOfWork));
            service.AddTransient<IUserAlbumRepository, UserAlbumRepository>();
            service.AddTransient<IUserAlbumMediaRepository, UserAlbumMediaRepository>();
            service.AddTransient<IUserRepository, UserRepository>();

            return service;
        }
    }
}
