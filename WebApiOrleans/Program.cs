// WebHost with Orleans Silo

using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// aspire dashboard standalone
builder.Services.AddOpenTelemetry().UseOtlpExporter(OtlpExportProtocol.Grpc, new Uri(oltpEndpoint));

// Orleans Host with Dashboard and some services - SILO
// https://learn.microsoft.com/en-us/dotnet/orleans/host/configuration-guide/adonet-configuration
builder.Host.UseOrleans(hostBuilder =>
{
    const string invariant = "System.Data.SqlClient";

    hostBuilder
        .UseLocalhostClustering()
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
        //.UseDashboard() // available at: http://localhost:8080/
        .AddActivityPropagation()
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

app.Run();
