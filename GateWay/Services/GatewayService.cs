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

        public string Leader = "1";

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
                Console.WriteLine(leader);
                guid = leader.ToString();
            }
            return guid;
        }

        public HttpClient clientMaker(string id)
        {
            HttpClient client = new() { BaseAddress = new Uri(nodes[int.Parse(id)]) };
            return client;
        }


        public async Task<bool> IsLeaderValid()
        {
            string newLeader = await FindLeader();
            Console.WriteLine($"valid leader: {newLeader}, stored leader: {Leader}");
            return newLeader == Leader;
        }

        public async Task<Item> EventualGet(string key)
        {

            var randomNode = ChooseRandomIndex();

            HttpClient client = clientMaker(randomNode.ToString());
            return await client.GetFromJsonAsync<Item>($"EventualGet/{key}");
        }

        public async Task<Item> StrongGet(string key)
        {
            // Proxy StrongGet request to the leader
            //request
            if (await IsLeaderValid())
            {
                HttpClient client = clientMaker(Leader);
                return await client.GetFromJsonAsync<Item>($"StrongGet/{key}");
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
                    return await client.GetFromJsonAsync<Item>($"StrongGet/{key}");
                }
                else
                {
                    return new Item("", 0); // Leader not valid
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
            bool isValid = await IsLeaderValid();
            if (isValid)
            {
                HttpClient client = clientMaker(Leader);

                HttpResponseMessage response = await client.PostAsJsonAsync<LogObject>($"AddToLog", new LogObject(key, value));

                response.EnsureSuccessStatusCode();
            }
            else
            {
                Leader = await FindLeader();
                bool isValid2 = await IsLeaderValid();
                if (isValid2)
                {
                    HttpClient client = clientMaker(Leader);

                    HttpResponseMessage response = await client.PostAsJsonAsync<LogObject>($"AddToLog", new LogObject(key, value));

                    response.EnsureSuccessStatusCode();
                }
            }
        }
    }

}



