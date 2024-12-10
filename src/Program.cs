using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using spa.server.Logging;

namespace spa.server
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
            builder.Services.AddResponseCaching();
            builder.Services.AddResponseCompression();
            
            builder.WebHost.ConfigureKestrel((context, options) =>
            {
                options.ListenAnyIP(5184, listenOptions =>
                {
                    listenOptions.UseConnectionLogging();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            var policyCollection = new HeaderPolicyCollection()
                .AddContentSecurityPolicy(csp =>
                {
                    csp.AddDefaultSrc().Self();
                    var hostArray = builder.Configuration.GetSection("ContentSecurityPolicy:FrameAncestors").Get<string[]>();
                    if (hostArray == null) return;
                    foreach (var host in hostArray)
                        csp.AddFrameAncestors().Sources.Add(host.Trim());
                });
            app.UseResponseCaching();

            app.UseDefaultFiles();
            app.UseStaticFiles(options: new StaticFileOptions()
            {
                HttpsCompression = HttpsCompressionMode.Compress,
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=604800";
                }
            });
            
            app.MapControllers();

            var appVersion = builder.Configuration.GetValue<string>("spa:version");
            app.MapFallbackToFile("/index.html", options: new StaticFileOptions()
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers[HeaderNames.CacheControl] = "no-cache";
                }
            });

            app.Run();
        }
    }
}
