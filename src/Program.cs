using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using react.server.Logging;

namespace react.server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables();

            // Add services to the container.
            builder.Logging.ClearProviders();
            builder.Logging.AddColorConsoleLogger();

            builder.Services.AddControllers();
            
            builder.WebHost.ConfigureKestrel((context, options) =>
            {
                options.ListenAnyIP(5184, listenOptions =>
                {
                    listenOptions.UseConnectionLogging();
                });
            });

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
