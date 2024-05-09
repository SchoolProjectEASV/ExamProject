using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TracingService;

    public static class TracingService
    {
        public static OpenTelemetryBuilder Setup(this OpenTelemetryBuilder builder, string serviceName)
        {
            return builder.WithTracing(t =>
            { 
                t
                .AddSource(serviceName)
                .AddZipkinExporter(z =>
                {
                    z.Endpoint = new Uri("http://zipkin:9411/api/v2/spans");
                })
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName: serviceName))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation();
        });
    }
}
