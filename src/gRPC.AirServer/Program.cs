using gRPC.Server.GrpcServices;
using gRPC.Server.GrpcServices.Interceptor;
using gRPC.Server.Persistence;
using gRPC.Server.Persistence.EF;
using gRPC.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IRepository, DatabaseRepository>();
builder.Services.AddScoped<DataProvider>();

builder.Services.AddDbContext<AirQDbContext>(options =>
{
    options.UseSqlite("Data Source=mydatabase.db");
});

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<TracingInterceptor>();
});
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