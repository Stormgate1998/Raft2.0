namespace GateWay.Services
{

    public class LogObject
    {
        public LogObject(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public string key { get; set; }
        public string value { get; set; }

    }

    public class Item(string value, int log)
    {
        public string value = value;
        public int log = log;
    }

    public class Gateway(Dictionary<int, string> nodes, ILogger<Gateway> logger, bool isTesting = false)
    {
        Dictionary<int, string> nodes = nodes;
        private readonly ILogger<Gateway> logger = logger;

        public string Leader {get; set; }

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
            int result = 0;
            while (result <= 0)
            {
                result = rand.Next(1, nodes.Count + 1);
            }
            return result;
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

        public HttpClient clientMaker(string id)
        {
            Console.WriteLine(nodes[int.Parse(id)]);
            HttpClient client = new() { BaseAddress = new Uri(nodes[int.Parse(id)]) };
            return client;
        }


        public async Task<bool> IsLeaderValid()
        {
            string newLeader = await FindLeader();
            Console.WriteLine($"valid leader: {newLeader}, stored leader: {Leader}");
            Leader = newLeader;
            return newLeader == Leader;
        }

        public async Task<string> EventualGet(string key)
        {

            var randomNode = ChooseRandomIndex();
            Console.WriteLine($"Index: {randomNode}");

            HttpClient client = clientMaker(randomNode.ToString());

            var response = await client.GetAsync($"Node/EventualGet/{key}");
            string result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);
            return result;
        }

        public async Task<string> StrongGet(string key)
        {
            // Proxy StrongGet request to the leader
            //request
            if (await IsLeaderValid())
            {
                HttpClient client = clientMaker(Leader);
                var response = await client.GetAsync($"Node/StrongGet/{key}");
                string result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
                return result;
            }
            else
            {
                if (Leader.Length == 0)
                {
                    Leader = await FindLeader();
                }
                if (await IsLeaderValid())
                {
                    HttpClient client = clientMaker(Leader);
                    var response = await client.GetAsync($"Node/StrongGet/{key}");
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(result);
                    return result;
                }
                else
                {
                    return "0,0"; // Leader not valid
                }
            }
        }

        public async Task<bool> CompareVersionAndSwap(string key, string newValue, int expectedVersion)
        {
            if (await IsLeaderValid())
            {
                HttpClient client = clientMaker(Leader);

                HttpResponseMessage response = await client.PostAsync($"Node/CompareVersionAndSwap/{key}/{newValue}/{expectedVersion}", null);

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

                    HttpResponseMessage response = await client.PostAsync($"Node/CompareVersionAndSwap/{key}/{newValue}/{expectedVersion}", null);

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

        public async Task AddToLog(LogObject logObject)
        {
            bool isValid = await IsLeaderValid();
            if (isValid)
            {
                HttpClient client = clientMaker(Leader);
                Console.WriteLine(client.BaseAddress);
                Console.WriteLine($"Node/AddToLog/{logObject.key}, {logObject.value}");
                HttpResponseMessage response = await client.PostAsync($"Node/AddToLog/{logObject.key}/{logObject.value}", null);
                Console.WriteLine(response.Content.ToString());
                response.EnsureSuccessStatusCode();
            }
            else
            {
                Leader = await FindLeader();
                isValid = await IsLeaderValid();
                if (isValid)
                {
                    HttpClient client = clientMaker(Leader);

                    HttpResponseMessage response = await client.PostAsync($"Node/AddToLog/{logObject.key}/{logObject.value}", null);
                    Console.WriteLine(response.Content.ToString());
                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }

}



