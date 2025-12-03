using System.Text.Json;
using backend.Models;
using backend.PeriodicTasks;
using backend.PeriodicTasks.Tasks;
using backend.Repositories;
using backend.TelemetryService;
using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using SpaceApp.Options;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("ApplicationDatabase")!;
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseNpgsql(connectionString)
);

builder.Services.Configure<SpaceOptions>(
    builder.Configuration.GetSection("SpaceOptions"));

builder.Services.AddScoped<ISpaceRepository, SpaceRepository>();
builder.Services.AddScoped<IIssRepository, IssRepository>();
builder.Services.AddScoped<IOsdrRepository, OsdrRepository>();
    
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IHttpService, HttpService>();
builder.Services.AddSingleton<ApiSender>();

builder.Services.AddScoped<ITelemetryFormatter, TelemetryFormatter>();
builder.Services.AddScoped<IFileGeneratorCsv, CsvGenerator>();
builder.Services.AddScoped<IFileGeneratorXlsx, XlsxGenerator>();

builder.Services.AddScoped<FetchApodTask>();
builder.Services.AddScoped<FetchNeoTask>();
builder.Services.AddScoped<FetchSpacexTask>();
builder.Services.AddScoped<FetchFlrTask>();
builder.Services.AddScoped<FetchCmeTask>();
builder.Services.AddScoped<FetchOsdrTask>();
builder.Services.AddScoped<FetchIssTask>();
builder.Services.AddScoped<TelemetryCsvTask>();

builder.Services.AddScoped<IPeriodicTask, FetchApodTask>();
builder.Services.AddScoped<IPeriodicTask, FetchNeoTask>();
builder.Services.AddScoped<IPeriodicTask, FetchSpacexTask>();
builder.Services.AddScoped<IPeriodicTask, FetchFlrTask>();
builder.Services.AddScoped<IPeriodicTask, FetchCmeTask>();
builder.Services.AddScoped<IPeriodicTask, FetchOsdrTask>();
builder.Services.AddScoped<IPeriodicTask, FetchIssTask>();
builder.Services.AddScoped<IPeriodicTask, TelemetryCsvTask>();

builder.Services.AddHostedService<PeriodicTaskHostedService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Defect Managment Project API",
        Version = "v1"
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = false;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
    
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // http://localhost:5022/swagger/index.html
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Defect Managment Project API");
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

PrepareDb.PrepareDatabase(app);

app.Run();