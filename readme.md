# Orelans with .NET 9.0 samples

Configuration required to run samples:

- Create database named 'OrleansDB' or just use in memory provider
- Execute SQL files in the follwoing order:
    - SQLServer-Main.sql
        - In case of error execute 'CREATE TABLE' only
    - SQLServer-Clustering.sql
    - SQLServer-Persistence.sql
    - SQLServer-Reminders.sql


## Observability and Diagnostics

- [OpenTelemetry for .NET](https://opentelemetry.io/docs/languages/net/instrumentation/)

### Jaeger with OpenTelemetry (docker)

```powershell
docker run --name jaeger `
  -p 16686:16686 `
  -p 4317:4317 `
  -p 4318:4318 `
  -p 5778:5778 `
  -p 9411:9411 `
  jaegertracing/jaeger:2.2.0
```

### Standalone .NET Aspire dashboard (docker)

```powershell
docker run --rm -it -d `
    -p 18888:18888 `
    -p 4317:18889 `
    --name aspire-dashboard `
    mcr.microsoft.com/dotnet/aspire-dashboard:9.0
```

- [more information](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/standalone?tabs=powershell)

### Orleans Dashboard

- [OrleansDashboard](https://github.com/OrleansContrib/OrleansDashboard)

### Orleans with .NET Aspire

- [.NET Aspire Orleans integration](https://learn.microsoft.com/en-us/dotnet/aspire/frameworks/orleans?tabs=dotnet-cli)
- [.NET Aspire Orleans sample app](https://github.com/dotnet/aspire-samples/tree/main/samples/OrleansVoting)

## Knolwdge base

- [Develop Grains](https://learn.microsoft.com/en-us/dotnet/orleans/grains/)
- [Grain Persistance](https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-persistence/?pivots=orleans-7-0)
- [Grain Identity](https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-identity)
- [Grain Placement](https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-placement)
- [Grain Extensions](https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-extensions)
- [Timers and Reminders](https://learn.microsoft.com/en-us/dotnet/orleans/grains/timers-and-reminders)
- [Observers](https://learn.microsoft.com/en-us/dotnet/orleans/grains/observers)


- [Reddit Q&A](https://www.reddit.com/r/dotnet/comments/16kk2l1/is_orleans_any_good/)
