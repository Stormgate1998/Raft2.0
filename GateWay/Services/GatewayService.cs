namespace GateWay.Services
{

    public class Gateway(List<string> nodes, ILogger<Gateway> logger)
    {
        List<string> nodes = nodes;
        private readonly ILogger<Gateway> logger = logger;


        public async Task<string> ReturnIdOfNodeAsync(string id)
        {
            HttpClient client = new() { BaseAddress = new Uri($"http://josh-node-{id}:8080") };
            var response = await client.GetFromJsonAsync<int>($"Node/GetId");

            return response.ToString();
        }

        public async Task<string> ReturnIdFromSecondNode(string firstNode, string secondNode)
        {
            HttpClient client = new() { BaseAddress = new Uri($"http://josh-node-{firstNode}:8080") };
            var response = await client.GetFromJsonAsync<int>($"Node/GetOtherId/{secondNode}");

            return response.ToString();
        }

        public List<string> ReturnList()
        {
            return nodes;
        }
    }




}
