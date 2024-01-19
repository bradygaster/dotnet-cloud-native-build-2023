using Orders;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDataSource("ordersDb");
builder.Services.AddHostedService<DatabaseInitializer>();
builder.Services.AddSingleton<IOrdersDb, OrdersDb>();
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
