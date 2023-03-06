using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using AppSample.CoreTools.ConfigureServices.OpenTelemetry;
using AppSample.CoreTools.ConfigureServices.OpenTelemetry.Settings;

namespace AppSample.CoreTools.ConfigureServices
{
    public static class ServiceCollectionExtensions
    {
        public static void AddOpenTelemetry(this IServiceCollection services, TelemetrySettings settings)
        {
	        if (settings.Tracing.IsEnabled)
	        {
		        services.AddOpenTelemetryTracing(builder =>
		        {
			        if (settings.Tracing.RewriteHttpClientSpanName)
			        {
				        builder.AddProcessor(new HttpClientSpanNameEnrichingProcessor());
			        }

			        builder
				        .AddJaegerExporter(options =>
				        {
					        options.AgentHost = settings.Tracing.Jaeger.AgentHost;
					        options.AgentPort = settings.Tracing.Jaeger.AgentPort;
				        })
				        .AddSource(settings.Source)
				        .SetResourceBuilder(ResourceBuilder
					        .CreateDefault()
					        .AddService(settings.Source)
					        .AddTelemetrySdk());

			        if (settings.Tracing.HttpClientInstrumentation)
			        {
				        builder.AddHttpClientInstrumentation(options =>
				        {
					        options.RecordException = settings.Tracing.RecordException;

					        if (settings.Tracing.HttpClientInstrumentationFilter.Any())
					        {
						        options.Filter = req =>
							        req.RequestUri == null
							        || settings.Tracing.HttpClientInstrumentationFilter
								        .All(f => !req.RequestUri.AbsolutePath.Contains(f,
									        StringComparison.OrdinalIgnoreCase));
					        }
				        });
			        }

			        if (settings.Tracing.SqlClientInstrumentation)
			        {
				        builder.AddSqlClientInstrumentation(options =>
				        {
					        options.RecordException = settings.Tracing.RecordException;
					        options.SetDbStatementForText = settings.Tracing.SetDbStatementForText;
				        });
			        }

			        if (settings.Tracing.AspNetCoreInstrumentation)
			        {
				        builder.AddAspNetCoreInstrumentation(options =>
				        {
					        if (settings.Tracing.AspNetCoreInstrumentationFilter.Any())
					        {
						        options.Filter = req =>
						        {
							        var uri = req.Request.Path.ToUriComponent();

							        return settings.Tracing.AspNetCoreInstrumentationFilter
								        .All(f => !uri.Contains(f, StringComparison.OrdinalIgnoreCase));
						        };
					        }
					        
					        options.RecordException = settings.Tracing.RecordException;
				        });
			        }
		        });
	        }

	        if (settings.Metrics.IsEnabled)
	        {
		        services.AddOpenTelemetryMetrics(builder =>
		        {
			        builder
				        .AddPrometheusExporter()
				        .SetResourceBuilder(ResourceBuilder
					        .CreateDefault()
					        .AddService(settings.Source)
					        .AddTelemetrySdk());

			        if (settings.Metrics.HttpClientInstrumentation)
			        {
				        builder.AddHttpClientInstrumentation();
			        }
			        
			        if (settings.Metrics.AspNetCoreInstrumentation)
			        {
				        builder.AddAspNetCoreInstrumentation();
			        }
			        
			        builder.AddMeter(settings.Source);
		        });
	        }

	        services.AddSingleton(_ => new Meter(settings.Source));
		}
    }
}
