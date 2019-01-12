using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Services;
namespace Service
{
    public class ServiceRegistration
    {
        private readonly IServiceCollection services;
        private readonly IConfiguration configuration;

        public ServiceRegistration(IServiceCollection s, IConfiguration c)
        {
            services = s;
            configuration = c;
        }

        public void Register()
        {
            services.Configure<DbConnectionConfig>(configuration.GetSection("DbConnectionConfig"));
            services.AddSingleton<UserConnectionManager>();
            services.AddSingleton<AdminConnection>();
            services.AddSingleton<QueryParser>();
        }
    }
}
