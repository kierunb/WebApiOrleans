// WebHost with Orleans Silo

using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orleans.Configuration;
using Orleans.Hosting;
using WebApiOrleans.Grains;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// app services
builder.Services.AddTransient<IChatObserver, ChatObserver>();

// OpenTelemetry config
var serviceName = builder.Configuration["OpenTelemetry:ServiceName"]!;
var serviceVersion = builder.Configuration["OpenTelemetry:ServiceVersion"]!;
var oltpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"]!;

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

builder
    .Services.AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService(serviceName: serviceName, serviceVersion: serviceVersion)
    )
    .WithMetrics(metrics =>
    {
        metrics.AddMeter(serviceName);
        metrics.AddMeter("Microsoft.Orleans");
        metrics.AddHttpClientInstrumentation();
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddSqlClientInstrumentation();
        metrics.AddRuntimeInstrumentation();
        metrics.AddProcessInstrumentation();
    })
    .WithTracing(tracing =>
    {
        tracing.AddSource(serviceName);
        tracing.AddSource("Microsoft.Orleans.Runtime");
        tracing.AddSource("Microsoft.Orleans.Application");
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();
        tracing.AddSqlClientInstrumentation();
    });

// export to aspire dashboard standalone
builder
    .Services.AddOpenTelemetry()
    .UseOtlpExporter(OtlpExportProtocol.Grpc, new Uri(oltpEndpoint));

// Orleans Host with Dashboard and some services - SILO
// https://learn.microsoft.com/en-us/dotnet/orleans/host/configuration-guide/adonet-configuration
builder.Host.UseOrleans(hostBuilder =>
{
    const string invariant = "System.Data.SqlClient";

    hostBuilder
        .UseLocalhostClustering()
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "dev-cluster";
            options.ServiceId = "dev-cluster";
        })
        .AddMemoryGrainStorage("memory")
        .AddAdoNetGrainStorage(
            "db",
            options =>
            {
                options.Invariant = invariant;
                options.ConnectionString = builder.Configuration.GetConnectionString("OrleansDB");
            }
        )
        //.UseInMemoryReminderService()
        .UseAdoNetReminderService(options =>
        {
            options.Invariant = invariant;
            options.ConnectionString = builder.Configuration.GetConnectionString("OrleansDB");
        })
        //.UseDashboard() // Orleans Dasboard available at: http://localhost:8080/
        .UseDashboard(options => { options.HostSelf = true; })
        .AddActivityPropagation() // required for tracing
        .ConfigureLogging(logging => logging.AddConsole());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Map("/dashboard", x => x.UseOrleansDashboard());    // self-hosted Orleans Dashboard

app.MapGet(
    "/shorten",
    static async (IGrainFactory grains, HttpRequest request, string url) =>
    {
        var host = $"{request.Scheme}://{request.Host.Value}";

        // Validate the URL query string.
        if (
            string.IsNullOrWhiteSpace(url)
            || Uri.IsWellFormedUriString(url, UriKind.Absolute) is false
        )
        {
            return Results.BadRequest(
                $"""
                The URL query string is required and needs to be well formed.
                Consider, ${host}/shorten?url=https://www.microsoft.com.
                """
            );
        }

        // Create a unique, short ID
        var shortenedRouteSegment = Guid.NewGuid().GetHashCode().ToString("X");

        // Create and persist a grain with the shortened ID and full URL
        var shortenerGrain = grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);

        await shortenerGrain.SetUrl(url);

        // Return the shortened URL for later use
        var resultBuilder = new UriBuilder(host) { Path = $"/go/{shortenedRouteSegment}" };

        return Results.Ok(resultBuilder.Uri);
    }
);

app.MapGet(
    "/go/{shortenedRouteSegment:required}",
    static async (IGrainFactory grains, string shortenedRouteSegment) =>
    {
        // Retrieve the grain using the shortened ID and url to the original URL
        var shortenerGrain = grains.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);

        var url = await shortenerGrain.GetUrl();

        // Handles missing schemes, defaults to "http://".
        var redirectBuilder = new UriBuilder(url);

        return Results.Redirect(redirectBuilder.Uri.ToString());
    }
);

app.Run();
