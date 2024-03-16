using RaftNode.Controllers;
using RaftNode.Services;

var builder = WebApplication.CreateBuilder(args);


string myNode = builder.Configuration.GetSection("SERVER_NAME").Value ?? "0";
string list = builder.Configuration.GetSection("LIST_OF_NODES").Value ?? "";

string[] pairs = list.Split(';');

// Dictionary to store key-value pairs
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
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use property names as they are defined in the class
    });

builder.Services.AddSingleton<NodeService>(x =>
{
    return new NodeService(myNode, keyValuePairs);
});

builder.Services.AddSingleton<NodeController>(provider =>
{
    return new NodeController(provider.GetRequiredService<NodeService>());
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
    return keyValuePairs;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapControllers();

app.Run();
