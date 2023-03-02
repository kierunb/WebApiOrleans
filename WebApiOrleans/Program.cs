// WebHost with Orleans Silo

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Orleans Host with Dashboard and some services - SILOS
// https://learn.microsoft.com/en-us/dotnet/orleans/host/configuration-guide/adonet-configuration

builder.Host.UseOrleans(hostBuilder =>
{
    const string invariant = "System.Data.SqlClient";

    hostBuilder
        .UseLocalhostClustering()
        .AddMemoryGrainStorage("memory")
        .AddAdoNetGrainStorage("db", options =>
        {
            options.Invariant = invariant;
            options.ConnectionString = builder.Configuration.GetConnectionString("OrleansDB");
        })
        //.UseInMemoryReminderService()
        .UseAdoNetReminderService(options =>
        {
            options.Invariant = invariant;
            options.ConnectionString = builder.Configuration.GetConnectionString("OrleansDB");
        })
        .UseDashboard()     // available at: http://localhost:8080/
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
