using Azure.Identity;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;
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

            builder.Services.AddApplicationInsightsTelemetry();
            
            // Add services to the container.
            builder.Logging.ClearProviders();
            builder.Logging.AddColorConsoleLogger();
            
            builder.Services.AddFeatureManagement()
                .WithTargeting()
                .AddApplicationInsightsTelemetry();

            var appConfigurationConnectionString = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration") ?? "";
            if (!string.IsNullOrEmpty(appConfigurationConnectionString))
            {
                builder.Configuration.AddAzureAppConfiguration(options =>
                {
                    options.Connect(new Uri(appConfigurationConnectionString), new DefaultAzureCredential())
                        .Select("Spa:*", LabelFilter.Null)
                        .ConfigureRefresh(refreshOptions =>
                            refreshOptions.Register("Spa:Sentinel", refreshAll:true));
                    options.UseFeatureFlags(featureFlagOptions =>
                    {
                        featureFlagOptions.SetRefreshInterval(TimeSpan.FromMinutes(5));
                    });
                });
            }
            builder.AddServiceDefaults();
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

            app.MapDefaultEndpoints();

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

            app.MapHealthChecks("/api/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(delegate(Options options)
            {
                options.UIPath = "/healthchecks-ui";
            });

            var appVersion = builder.Configuration.GetValue<string>("spa:version");
            app.MapFallbackToFile("/index.html", options: new StaticFileOptions()
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers[HeaderNames.CacheControl] = "no-cache";
                    context.Context.Response.Headers["X-App-Version"] = appVersion;
                }
            });

            app.UseMiddleware<TargetingHttpContextMiddleware>();

            app.Run();
        }
    }
}
