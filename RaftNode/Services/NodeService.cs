namespace RaftNode.Services
{

    public class Node(string id, Dictionary<int, string> nodes)
    {
        Dictionary<int, string> nodes = nodes;
        readonly string id = id;


        public string ReturnId()
        {
            return id;
        }

        public async Task<string> ReturnOtherId(string otherId)
        {
            HttpClient client = new() { BaseAddress = new Uri(nodes[int.Parse(otherId)]) };
            var response = await client.GetFromJsonAsync<int>($"Node/GetId");

            return response.ToString();
        }

        public Dictionary<int, string> ReturnList()
        {
            return nodes;
        }
    }




}
