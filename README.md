# FloorPlanner API

A REST API for managing floor plans (rooms, doors, windows, furniture) with automatic validation of furniture placement. Built with ASP.NET Core 11, EF Core and PostgreSQL. The pure domain logic (geometry, area calculation, placement rules) lives in a separate library with no framework dependencies.

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

## DI and IoC

A dependency injection (DI) container is the object that creates the application's services and passes ready-made dependencies into their constructors. This is inversion of control (IoC): a class does not create the objects it needs with `new`, it only declares them, and the framework decides what to hand over. Code then depends on interfaces (`IRepository<T>`, `IPlacementValidator`) and is easy to replace in tests.

A service is registered with one of three lifetimes. Transient: a new instance on every request to the container. Scoped: one instance per HTTP request. Singleton: one instance for the whole life of the application.

`AppDbContext` and `EfRepository<T>` are Scoped: each request gets its own context, and its tracked changes are saved or discarded together. A `DbContext` is not thread-safe, so it must not be shared between requests. `PlacementValidator` is a Singleton because it holds no state, and one instance can safely serve all requests.

The rule: a Singleton must not depend on a Scoped service. Otherwise, the long-lived object would keep a request's context after the request has ended.
