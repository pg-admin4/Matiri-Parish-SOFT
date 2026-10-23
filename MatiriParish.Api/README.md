MatiriParish.Api - Minimal ASP.NET Core Web API

This project is a scaffolded API that uses the existing MatiriParish.Core project for models and the EF Core DbContext.

How to run locally (recommended):

1. Start a local Postgres database using Docker Compose:
   docker compose up -d

   This will start Postgres on port 5432 with database `matiri_parish_api`, user `postgres` and password `postgres`.

2. From the solution root, run the API:
   dotnet build MatiriParish.Api
   dotnet run --project MatiriParish.Api

   The API reads connection string from configuration or the environment variable `ConnectionStrings__Default`.

3. Open Swagger at https://localhost:5001/swagger to try endpoints.

Deployment to cloud (Supabase / ElephantSQL / Azure Database for PostgreSQL):
- Provision a Postgres instance and obtain its connection string.
- Set the connection string as an environment variable named `ConnectionStrings__Default` on the host.
- Deploy the MatiriParish.Api project (App Service, Docker container, etc.). The API will apply migrations on startup.

Security note: do NOT embed production credentials into appsettings.json. Use secrets / environment variables.
