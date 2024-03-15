using gRPC.Server.GrpcServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DataProvider>();

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