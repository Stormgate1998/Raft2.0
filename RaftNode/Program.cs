using RaftNode.Controllers;
using RaftNode.Services;

var builder = WebApplication.CreateBuilder(args);


string myNode = builder.Configuration.GetSection("SERVER_NAME").Value ?? "0";
string list = builder.Configuration.GetSection("LIST_OF_NODES").Value ?? "";
List<string> nodes = [.. list.Split(',')];
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddScoped<Node>(x =>
{
    return new Node(myNode, nodes);
});

builder.Services.AddScoped<NodeController>(provider =>
{
    return new NodeController(provider.GetRequiredService<Node>());
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();


app.MapGet("/listofnodes", () =>
{
    return nodes;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
