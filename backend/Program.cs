using System.Text.Json;
using backend.Models;
using backend.PeriodicTasks;
using backend.Repositories;
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
    
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IHttpService, HttpService>();

builder.Services.AddScoped<IPeriodicTask, FetchApodTask>();
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