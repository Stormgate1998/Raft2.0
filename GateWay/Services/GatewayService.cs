namespace GateWay.Services
{

    public class Gateway(Dictionary<int, string> nodes, ILogger<Gateway> logger, bool isTesting = false)
    {
        Dictionary<int, string> nodes = nodes;
        private readonly ILogger<Gateway> logger = logger;

        public string Leader = "";

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


        private int ChooseRandomIndex()
        {
            Random rand = new Random();
            return rand.Next(0, nodes.Count);
        }

        public async Task<string> FindLeader()
        {
            string? guid = null;
            while (guid == null)
            {
                int index = ChooseRandomIndex();

                HttpClient client = clientMaker(index.ToString());
                var leader = await client.GetFromJsonAsync<int>($"Node/FindLeader");
                guid = leader.ToString();
            }
            return guid;
        }

        public HttpClient clientMaker(string targetId)
        {
            string item = "";
            nodes.TryGetValue(int.Parse(targetId), out item);
            HttpClient client = new() { BaseAddress = new Uri(item) };
            return client;
        }


        public async Task<bool> IsLeaderValid()
        {
            string newLeader = await FindLeader();
            return newLeader == Leader;
        }

        public async Task<string?> EventualGet(string key)
        {

            var randomNode = ChooseRandomIndex();

            HttpClient client = clientMaker(randomNode.ToString());
            return await client.GetFromJsonAsync<string>($"EventualGet/{key}");
        }

        public async Task<string?> StrongGet(string key)
        {
            // Proxy StrongGet request to the leader
            //request
            if (await IsLeaderValid())
            {
                HttpClient client = clientMaker(Leader);
                return await client.GetFromJsonAsync<string>($"StrongGet/{key}");
            }
            else
            {
                Leader = await FindLeader();
                if (await IsLeaderValid())
                {
                    HttpClient client = clientMaker(Leader);
                    return await client.GetFromJsonAsync<string>($"StrongGet/{key}");
                }
                else
                {
                    return null; // Leader not valid
                }
            }
        }

        public async Task<bool> CompareVersionAndSwap(string key, string newValue, int expectedVersion)
        {
            if (await IsLeaderValid())
            {
                HttpClient client = clientMaker(Leader);

                HttpResponseMessage response = await client.PostAsync($"CompareVersionAndSwap/{key}/{newValue}/{expectedVersion}", null);

                response.EnsureSuccessStatusCode();

                bool success = bool.Parse(await response.Content.ReadAsStringAsync());
                return success;

            }
            else
            {
                Leader = await FindLeader();
                if (await IsLeaderValid())
                {
                    HttpClient client = clientMaker(Leader);

                    HttpResponseMessage response = await client.PostAsync($"CompareVersionAndSwap/{key}/{newValue}/{expectedVersion}", null);

                    response.EnsureSuccessStatusCode();

                    bool success = bool.Parse(await response.Content.ReadAsStringAsync());
                    return success;
                }
                else
                {
                    return false; // Leader not valid
                }
            }
        }

        public async Task AddToLog(string key, string value)
        {
            if (await IsLeaderValid())
            {
                HttpClient client = clientMaker(Leader);

                HttpResponseMessage response = await client.PostAsync($"AddToLog/{key}/{value}", null);

                response.EnsureSuccessStatusCode();
            }
            else
            {
                Leader = await FindLeader();
                if (await IsLeaderValid())
                {
                    HttpClient client = clientMaker(Leader);

                    HttpResponseMessage response = await client.PostAsync($"AddToLog/{key}/{value}", null);

                    response.EnsureSuccessStatusCode();

                }
            }
        }

    }


}
