using Codium.Template.HttpApi.Host.Logging.Enrichers;
using Codium.Template.HttpApi.Host.Logging.Models;
using Codium.Template.HttpApi.Host.Logging.Sinks;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Codium.Template.HttpApi.Host.Logging;

public static class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder,
        IConfiguration configuration)
    {
        var serilogOptions = new SerilogOptions();
        configuration.GetSection(SerilogOptions.SectionName).Bind(serilogOptions);

        if (serilogOptions.Enabled)
        {
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog((context, services, serilogConfiguration) =>
            {
                var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
                var httpContextEnricher = new HttpContextEnricher(httpContextAccessor);

                serilogConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithEnvironmentName()
                    .Enrich.With(httpContextEnricher);

                if (serilogOptions.Console.Enabled)
                {
                    if (serilogOptions.Console.IsCustom)
                    {
                        serilogConfiguration.WriteTo.CustomConsoleWriteToJson();
                    }
                    else
                    {
                        serilogConfiguration.WriteTo.Console(
                            outputTemplate: serilogOptions.Console.OutputTemplate,
                            theme: serilogOptions.Console.Theme,
                            restrictedToMinimumLevel: serilogOptions.Console.MinimumLevel
                        );
                    }
                }

                if (serilogOptions.File.Enabled)
                {
                    serilogConfiguration.WriteTo.File(
                        path: serilogOptions.File.PathToTxt,
                        rollingInterval: serilogOptions.File.RollingInterval,
                        outputTemplate: serilogOptions.File.OutputTemplate
                    );
                }
            });
        }

        return builder;
    }
}