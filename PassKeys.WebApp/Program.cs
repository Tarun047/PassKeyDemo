using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PassKeys.Business;
using PassKeys.WebApp.Authentication;
using PassKeys.WebApp.Extensions;
using PassKeys.WebApp.Middleware;

namespace PassKeys.WebApp;

public class Program
{
    static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PassKeysDbContext>(options => options.UseNpgsql(configuration.GetDbConnectionString()));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<TokenService>();
        
        services.AddFido2(options =>
        {
            options.ServerDomain = configuration["Fido2:ServerDomain"];
            options.ServerName = "Passkeys Demo App";
            options.Origins = configuration.GetSection("Fido2:Origins").Get<HashSet<string>>();
            options.TimestampDriftTolerance = configuration.GetValue<int>("Fido2:TimestampDriftTolerance");
            options.MDSCacheDirPath = configuration["Fido2:MDSCacheDirPath"];
        });
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "Pass Keys Demo";
        });
        
        services.AddControllers();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters.ValidIssuer = configuration["Jwt:Issuer"];
                options.TokenValidationParameters.ValidAudience = configuration["Jwt:Audience"];
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
                options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
            });
    }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder.Services, builder.Configuration);
        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<DeviceDetectionMiddleware>();
        app.MapControllers().RequireAuthorization();

        if (app.Environment.IsDockerDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PassKeysDbContext>();
            dbContext.Database.Migrate();
        }

        app.Run();
    }
}