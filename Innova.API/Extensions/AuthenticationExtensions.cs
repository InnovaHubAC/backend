using Innova.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Innova.API.Extensions
{
    public static class AuthenticationExtensions
    {
        public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtOptions = configuration.GetSection("Jwt");
            services.Configure<JwtSettings>(jwtOptions);
            var jwt = jwtOptions.Get<JwtSettings>();

            var googleOptions = configuration.GetSection("Authentication:Google");
            services.Configure<GoogleAuthSettings>(googleOptions);
            var googleSettings = googleOptions.Get<GoogleAuthSettings>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddCookie().AddGoogle(options =>
            {
                options.ClientId = googleSettings!.ClientId;
                options.ClientSecret = googleSettings!.ClientSecret;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }
            )
            .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                        ClockSkew = TimeSpan.Zero
                    };
                });
        }
    }
}
