# FloorPlanner API

A REST API for managing floor plans (rooms, doors, windows, furniture) with automatic validation of furniture placement. Built with ASP.NET Core 8, EF Core and PostgreSQL. The pure domain logic (geometry, area calculation, placement rules) lives in a separate library with no framework dependencies.

## Roadmap

- **A. Core** — OOP domain model and geometry (no database)
- **B. API, DB, DI and error handling** — Web API, EF Core + PostgreSQL, repositories, exception middleware, CRUD
- **C. LINQ and SQL** — furniture search, plan summaries, reporting query
- **D. Debugging and service tests** — failing test → fix, tests with a fake repository
- **E. Demo page and Docker** — single demo page, Dockerfile and compose

## Run

```bash
docker compose up -d db
dotnet run --project src/FloorPlanner.Api
```

## API documentation (Swagger)

The API uses [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore), which generates an OpenAPI description of all endpoints from the controllers and serves it with Swagger UI. Nothing has to be written by hand: new endpoints appear in the documentation automatically.

- Swagger UI, where every endpoint can be viewed and called: http://localhost:5080/swagger
- The raw OpenAPI description (JSON): http://localhost:5080/swagger/v1/swagger.json

Swagger is controlled by the `Swagger:Enabled` setting. It is `true` in `appsettings.Development.json` (the default for `dotnet run`) and `false` in `appsettings.json`. To enable it in another environment, set the environment variable `Swagger__Enabled=true`.

## Database

The schema is created by the EF Core migration `Initial` (`src/FloorPlanner.Api/Data/Migrations`). With `Database:MigrateOnStartup` set to `true` the application applies it at startup.

Tables after the first start (`docker exec -it floorplanner-db psql -U floorplanner -d floorplanner -c "\dt"`):

```text
                   List of relations
 Schema |         Name          | Type  |    Owner
--------+-----------------------+-------+--------------
 public | __EFMigrationsHistory | table | floorplanner
 public | floor_plans           | table | floorplanner
 public | plan_objects          | table | floorplanner
 public | rooms                 | table | floorplanner
(4 rows)
```

All plan objects (furniture, doors, windows) share the `plan_objects` table (table per hierarchy); the `kind` column holds the discriminator and `category` is filled only for furniture. The room corners are stored in `rooms.corners` as `jsonb`:

```text
 Table "public.plan_objects"            Table "public.rooms"
  Column  |          Type                 Column    |          Type
----------+------------------------    ---------------+------------------------
 kind     | character varying(13)        corners       | jsonb
 category | character varying(50)
```
