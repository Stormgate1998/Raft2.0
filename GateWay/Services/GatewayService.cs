namespace GateWay.Services
{

    public class Gateway(Dictionary<int, string> nodes, ILogger<Gateway> logger, bool isTesting = false)
    {
        Dictionary<int, string> nodes = nodes;
        private readonly ILogger<Gateway> logger = logger;


        public async Task<string> ReturnIdOfNodeAsync(string id)
        {
            HttpClient client;
            if (!isTesting)
            {
                client = new() { BaseAddress = new Uri(nodes[int.Parse(id)]) };
            }
            else
            {
                client = new() { BaseAddress = new Uri($"http://josh-test-node-{id}:8080") };
            }
            var response = await client.GetFromJsonAsync<int>($"Node/GetId");

            return response.ToString();
        }

        public async Task<string> ReturnIdFromSecondNode(string firstNode, string secondNode)
        {
            HttpClient client;
            if (!isTesting)
            {
                client = new() { BaseAddress = new Uri(nodes[int.Parse(firstNode)]) };
            }
            else
            {
                client = new() { BaseAddress = new Uri($"http://josh-test-node-{firstNode}:8080") };
            }
            var response = await client.GetFromJsonAsync<int>($"Node/GetOtherId/{secondNode}");

            return response.ToString();
        }

        public Dictionary<int, string> ReturnList()
        {
            return nodes;
        }
    }




}
