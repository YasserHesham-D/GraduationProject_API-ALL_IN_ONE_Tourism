using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Presentation.ServiceExtensions
{


    public static class CustomJWTAuthenticationExtension
    {

        public static void CustomJwtAuth(this IServiceCollection services, IConfiguration configuration)
        {
            var jWT =  configuration.GetSection("JWT");

            services.AddSingleton(jWT);

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(Options =>
            {
                Options.RequireHttpsMetadata = false;

                Options.SaveToken = true;

                Options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = jWT.GetSection("Issuer").Value,

                    ValidateAudience = true,
                    ValidAudience = jWT.GetSection("Audience").Value,

                    ValidateLifetime = true,
                    
                    ClockSkew = TimeSpan.Zero,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jWT.GetSection("SigningKey").Value)),

                    RequireExpirationTime = true,
                    RequireSignedTokens = true

                };

                Options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully");
                        return Task.CompletedTask;
                    }
                };

            });
        }
    }

}
