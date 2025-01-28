# Orelans with .NET 9.0 samples

Configuration required to run example:
- Create database named 'OrleansDB'
- Execute SQL files in the follwoing order:
    - SQLServer-Main.sql
        - In case of error execute 'CREATE TABLE' only
    - SQLServer-Clustering.sql
    - SQLServer-Persistence.sql
    - SQLServer-Reminders.sql



// Jaeger docker
docker run --name jaeger `
  -p 16686:16686 `
  -p 4317:4317 `
  -p 4318:4318 `
  -p 5778:5778 `
  -p 9411:9411 `
  jaegertracing/jaeger:2.2.0