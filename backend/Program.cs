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

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

PrepareDb.PrepareDatabase(app);

app.Run();