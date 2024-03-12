namespace RaftNode.Services
{

    public class Node(string id, List<string> nodes)
    {
        List<string> nodes = nodes;
        readonly string id = id;


        public string ReturnId()
        {
            return id;
        }

        public async Task<string> ReturnOtherId(string otherId)
        {
            HttpClient client = new() { BaseAddress = new Uri($"http://josh-node-{otherId}:8080") };
            var response = await client.GetFromJsonAsync<int>($"Node/GetId");

            return response.ToString();
        }

        public List<string> ReturnList()
        {
            return nodes;
        }
    }




}
