using Microsoft.EntityFrameworkCore;
using Orders;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<OrdersDbContext>("ordersdb", null,
    optionsBuilder => optionsBuilder.UseNpgsql(npgsqlBuilder =>
        npgsqlBuilder.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)));
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DatabaseInitializer>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Orders");

if (app.Environment.IsDevelopment())
{
    app.MapGet("/envvars", () =>
    {
        var envVars = Environment.GetEnvironmentVariables();
        var envVarsString = string.Join("\n", envVars.Keys.Cast<string>().Select(key => $"{key}={envVars[key]}"));
        return envVarsString;
    });
    
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapOrdersApi();

app.Run();
