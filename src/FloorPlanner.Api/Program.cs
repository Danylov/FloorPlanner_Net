using FloorPlanner.Api.Data;
using FloorPlanner.Api.Endpoints;
using FloorPlanner.Api.Middleware;
using FloorPlanner.Api.Services;
using FloorPlanner.Core.Abstractions;
using FloorPlanner.Core.Placement;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

// Registration of our own services in the DI container: the database context, the repository, the placement validator,
// the furniture service and the plan summary service. Classes that need them get them through their constructors.
//
// When the dependencies are substituted: our code never calls "new FloorPlansController(plans, summaries)". The registrations below
// are only the rules, and the substitution is made by ASP.NET Core while it handles a request (AddControllers and MapControllers
// connect the controllers). When a request for api/floorplans... arrives, ASP.NET Core:
// 1. creates the FloorPlansController object;
// 2. looks at the parameters of its constructor (IRepository<FloorPlan> plans, IPlanSummaryService summaries);
// 3. for each parameter takes the implementation by the rules below (EfRepository<FloorPlan> and PlanSummaryService)
//    and creates it, substituting the dependencies that the implementation needs itself (for example, AppDbContext);
// 4. calls the constructor and then the required action of the controller.
// The objects are created a new for every request (Scoped registration) and are released after the response is sent.
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString).UseSnakeCaseNamingConvention()); // IoC container setup: dependency registration, Scoped
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>)); // IoC container setup: dependency registration, Scoped: interface -> implementation (uses the scoped AppDbContext)
builder.Services.AddSingleton<IPlacementValidator, PlacementValidator>(); // IoC container setup: dependency registration, Singleton: no state
builder.Services.AddScoped<IFurnitureService, FurnitureService>(); // IoC container setup: dependency registration, Scoped: interface -> implementation (uses scoped repositories)
builder.Services.AddScoped<IPlanSummaryService, PlanSummaryService>(); // IoC container setup: dependency registration, Scoped: interface -> implementation (uses the scoped repository)

// Registration of the framework services that the web API needs: the controllers, the validation of request bodies
// for the Minimal API endpoints (the controllers validate them themselves), and the generator of the Swagger documentation
// (AddEndpointsApiExplorer lets it see the endpoints, AddSwaggerGen creates the description of the API).
builder.Services.AddControllers();
builder.Services.AddValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Startup step: if the Database:MigrateOnStartup flag is on, apply the pending EF Core migrations asynchronously
// before the application starts to serve requests. The scope is needed because AppDbContext is a Scoped service.
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using IServiceScope scope = app.Services.CreateScope();
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
}

// Startup step: if the Database:SeedDemoData flag is on, fill an empty database with the demo plan (after the migration).
if (app.Configuration.GetValue<bool>("Database:SeedDemoData"))
{
    using IServiceScope scope = app.Services.CreateScope();
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await SeedData.InitializeAsync(db, app.Lifetime.ApplicationStopping);
}

// Middleware code starts here: the components of the request pipeline, which run in the order they are added.
// Pipeline order: the exception handler goes first so that it catches exceptions thrown by every later layer;
// then Swagger; then the static files (the demo page); then the endpoints.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// The demo page: UseDefaultFiles serves wwwroot/index.html for the address "/", UseStaticFiles serves the files of wwwroot.
app.UseDefaultFiles();
app.UseStaticFiles();

// Registration of the endpoints (the routes that the pipeline ends with): MapControllers adds the routes of the controller
// classes (health, floor plans, rooms); MapFurnitureEndpoints and MapFurnitureSearchEndpoints add the Minimal API routes of the furniture.
app.MapControllers();
app.MapFurnitureEndpoints();
app.MapFurnitureSearchEndpoints();

app.Run();
