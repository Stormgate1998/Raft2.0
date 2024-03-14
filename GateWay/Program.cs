using GateWay.Controllers;
using GateWay.Services;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
string list = builder.Configuration.GetSection("LIST_OF_NODES").Value ?? "";

string[] pairs = list.Split(';');

// Dictionary  to store key-value pairs
Dictionary<int, string> keyValuePairs = new Dictionary<int, string>();

foreach (string pair in pairs)
{
    // Split each pair by equal sign to separate key and value
    string[] parts = pair.Split('=');
    if (parts.Length == 2)
    {
        // Parse key and add to dictionary
        int key;
        if (int.TryParse(parts[0], out key))
        {
            // Add key-value pair to dictionary
            keyValuePairs.Add(key, parts[1]);
        }
    } 
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();


builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(
        ResourceBuilder
            .CreateDefault()
            .AddService("GateWay-Logs")
        )
        .AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri("http://jz-otel-collector:4317");
        }).AddConsoleExporter();
});



builder.Services.AddScoped<Gateway>(x =>
{
    var logger = x.GetRequiredService<ILogger<Gateway>>();

    return new Gateway(keyValuePairs, logger);
});

builder.Services.AddScoped<GatewayController>(provider =>
{
    return new GatewayController(provider.GetRequiredService<Gateway>());
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
