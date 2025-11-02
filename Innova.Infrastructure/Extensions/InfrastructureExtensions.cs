using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Innova.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static void ConfigureInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            
        }
    }
}