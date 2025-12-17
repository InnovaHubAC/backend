using Microsoft.Extensions.Hosting;
using Serilog;

namespace Innova.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static void ConfigureInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IHostBuilder host)
    {
        services.ConfigureDatabaseServices(configuration);
        services.ConfigureIdentityServices();
        services.ConfigureExternalServices(configuration);
        services.ConfigureCachingServices();
        services.ConfigureMessagingServices();
        ConfigureSerilogForHost(host);
    }

    private static void ConfigureSerilogForHost(IHostBuilder host)
    {
        host.UseSerilog((context, services, configuration) =>
        {
            configuration.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext();
        });
    }

    private static void ConfigureDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void ConfigureIdentityServices(this IServiceCollection services)
    {
        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
    }

    private static void ConfigureExternalServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
    }

    private static void ConfigureCachingServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
    }

    private static void ConfigureMessagingServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IUserConnectionService, UserConnectionService>();
    }
}
