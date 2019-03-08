using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Service
{
    public class ServiceRegistration
    {
        private readonly IServiceCollection _services;
        private readonly IConfiguration _configuration;

        public ServiceRegistration(IServiceCollection s, IConfiguration c)
        {
            _services = s;
            _configuration = c;
        }

        public void Register()
        {
            _services.AddCors();

            _services.Configure<DbConnectionConfig>(_configuration.GetSection("DbConnectionConfig"));
            _services.AddSingleton<QueryParser>();
            _services.AddSingleton<UserConnectionManager>();
            _services.AddSingleton<IAdminConnection, PostgresAdminConnection>();
            _services.AddSingleton<IConnectionFactory, PostgresConnectionFactory>();
            _services.AddSingleton<ISchemaService, PostgresSchemaService>();
            
        }
    }
}
