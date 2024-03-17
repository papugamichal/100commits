using gRPC.Server.GrpcServices;
using gRPC.Server.Persistence;
using gRPC.Server.Persistence.EF;
using gRPC.Server.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Repository>();
builder.Services.AddSingleton<DataProvider>();

builder.Services.AddDbContext<AirQDbContext>(options =>
{
    var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    var dbPath = System.IO.Path.Join(path, "station_updates.db");

    var connStr = new SqliteConnectionStringBuilder().DataSource = dbPath;
    options.UseSqlite(connStr);
});

// Add services to the container.
builder.Services.AddGrpc();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapGrpcService<GreeterService>();
app.MapGrpcService<AirQConsumer>();
app.MapGrpcService<AirQProducer>();
app.MapGet("/", context => context.Response.WriteAsJsonAsync("Hello world!"));

app.Run();