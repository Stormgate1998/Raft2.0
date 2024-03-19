using FrontEndRaft;
using FrontEndRaft.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;



var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string GateWayUrl = builder.Configuration.GetSection("GATEWAY_URL").Value ?? "";
if (GateWayUrl == null || GateWayUrl == "${GATEWAY_URL}") GateWayUrl = "http://localhost:5161";


builder.Services.AddScoped<ApiService>(provider =>
{
    HttpClient httpClient = new HttpClient { BaseAddress = new Uri(GateWayUrl) };
    return new ApiService(httpClient);
});


await builder.Build().RunAsync();
