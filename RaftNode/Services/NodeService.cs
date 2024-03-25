using System.Timers;

namespace RaftNode.Services
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
    public class KeyValueItem(string value, int log)
    {
        public string value = value;
        public int log = log;
    }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class NodeService
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        private static readonly object fileLock = new();
        private string votedFor;
        private int term;

        public NodeService(string id, Dictionary<int, string> nodes)
        {
            Identifier = id;
            List = nodes;
            votedFor = "";

            // LogFileName = Path.Combine("information", $"{id}LeaderInformation.txt");

            string leader = "";
            // (term, leader, current) = ReadNumbersFromFile(LogFileName);
            Random random = new Random();
            WaitTime = random.Next(1500, 3500);
            ElectionCountdownTimer = new System.Timers.Timer(WaitTime);
            HeartbeatTimer = new System.Timers.Timer(1000)
            {
                AutoReset = true
            };

            HeartbeatTimer.Elapsed += SendHeartbeats;
            ElectionCountdownTimer.AutoReset = true;
            ElectionCountdownTimer.Elapsed += BecomeLeader;
            HeartbeatTimer.Enabled = true;
            ElectionCountdownTimer.Enabled = true;

            ElectionCountdownTimer.Start();
            HeartbeatTimer.Stop();
            CurrentRole = Role.FOLLOWER;
            currentLeader = leader;

            // ReadInLogFile();
        }

        Dictionary<int, string> List { get; set; }
        public string Identifier { get; set; }

        public Role CurrentRole { get; set; }
        public string currentLeader { get; set; }

        private Dictionary<string, (string, int)> keyValueLog = [];

        public int WaitTime { get; set; }

        // public string LogFileName { get; set; }
        public System.Timers.Timer ElectionCountdownTimer { get; set; }
        public System.Timers.Timer HeartbeatTimer { get; set; }
        public string ReturnId()
        {
            return Identifier;
        }

        public string FindLeader()
        {
            return currentLeader;
        }

        public async Task<string> ReturnOtherId(string otherId)
        {
            HttpClient client = clientMaker(otherId);
            var response = await client.GetFromJsonAsync<int>($"Node/GetId");

            return response.ToString();
        }

        public HttpClient clientMaker(string targetId)
        {
            string item = "";
            List.TryGetValue(int.Parse(targetId), out item);
            HttpClient client = new() { BaseAddress = new Uri(item) };
            return client;
        }

        public Dictionary<int, string> ReturnList()
        {
            return List;
        }
        // private static (int, string, string) ReadNumbersFromFile(string filePath)
        // {

        //     lock (fileLock)
        //     {
        //         if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath) || new FileInfo(filePath).Length == 0)
        //         {
        //             // Create the file if it doesn't exist
        //             if (!File.Exists(filePath))
        //             {
        //                 File.WriteAllText(filePath, "");
        //             }
        //             // Return empty information
        //             return (0, "", "");
        //         }
        //         // Read numbers from file
        //         string[] numbers = File.ReadAllText(filePath).Split(',');
        //         int number1 = Convert.ToInt32(numbers[0]);
        //         string guid = numbers[1];
        //         string id = numbers[2];
        //         return (number1, guid, id);
        //     }
        // }

        // private void WriteNumbersToFile(int number1, string number2)
        // {
        //     string filePath = LogFileName;
        //     lock (fileLock)
        //     {
        //         // Check if the file is being read
        //         while (IsFileInUse(filePath))
        //         {
        //             Console.WriteLine("File is currently being read. Please try again later.");
        //             System.Threading.Thread.Sleep(250); // Wait for 1 second before retrying
        //         }

        //         // Write numbers to file
        //         string data = $"{number1},{number2},{this.Identifier}";
        //         File.WriteAllText(filePath, data);
        //     }
        // }

        // private static bool IsFileInUse(string filePath)
        // {
        //     try
        //     {
        //         using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        //         {
        //             fs.ReadByte();
        //         }
        //         return false;
        //     }
        //     catch (IOException)
        //     {
        //         return true;
        //     }
        // }

        private async void BecomeLeader(Object source, ElapsedEventArgs e)
        {
            await StartElectionAsync();
        }

        public bool? ProcessVoteRequest(string voterGuid, int term)
        { //ELEPHANT Edit for log thing

            // var (currentTerm, votedFor, _) = ReadNumbersFromFile(LogFileName);
            if (term > this.term)
            {
                if (CurrentRole != Role.FOLLOWER)
                {
                    HeartbeatTimer.Stop();
                    CurrentRole = Role.FOLLOWER;
                }
                // WriteNumbersToFile(term, voterGuid);
                votedFor = voterGuid;
                return true;
            }
            else if (term == this.term)
            {
                return voterGuid == votedFor;
            }
            return false;

        }

        public bool? ProcessHeartbeat(string leaderGuid, int term)
        {
            currentLeader = leaderGuid;
            // var (currentTerm, votedFor, _) = ReadNumbersFromFile(LogFileName);

            if (votedFor != leaderGuid || this.term != term)
            {
                this.term = term;
                votedFor = leaderGuid;
            }
            CurrentRole = Role.FOLLOWER;

            ElectionCountdownTimer.Stop();
            HeartbeatTimer.Stop();
            ElectionCountdownTimer.Start();
            return true;
        }

        private void SendHeartbeats(Object source, ElapsedEventArgs e)
        {
            if (currentLeader != Identifier)
            {
                currentLeader = Identifier;
            }
            _ = SendingHeartbeatAsync();
        }

        private async Task SendingHeartbeatAsync()
        {
            // var (currentTerm, votedFor, _) = ReadNumbersFromFile(LogFileName);

            Console.WriteLine("Sending Heartbeats");
            foreach (var item in List)
            {
                if (item.Key.ToString() != this.Identifier)
                {
                    HttpClient client = clientMaker(item.Key.ToString());
                    await client.PostAsync($"Node/ProcessHeartBeat/{Identifier}/{this.term}", null);
                }
            }
        }

        public Role GetRole()
        {
            Console.WriteLine($"Current role is: {CurrentRole}");
            return this.CurrentRole;
        }
        public async Task<int> StartElectionAsync(int UsedTerm = -1)
        {
            Console.WriteLine($"Beginning election with {Identifier} as candidate");

            ElectionCountdownTimer.Stop();

            // var (term, _, _) = ReadNumbersFromFile(LogFileName);
            term += 1;
            if (UsedTerm != -1)
            {
                term = UsedTerm;
                Console.WriteLine($"Term is {term}");

            }
            // WriteNumbersToFile(term, Identifier);

            int voteCount = 1;
            foreach (var node in List)
            {
                if (node.Key.ToString() != Identifier)
                {
                    Console.WriteLine($"Sending request to {node.Key}, url {node.Value}");

                    HttpClient client = clientMaker(node.Key.ToString());
                    bool? result = await client.GetFromJsonAsync<bool>($"Node/ProcessVoteRequest/{Identifier}/{term}");
                    Console.WriteLine($"got result of {result} from {node.Key}");

                    if (result != null && result != false)
                    {
                        voteCount++;
                        Console.WriteLine(voteCount);
                    }
                }
            }

            if (voteCount * 2 >= List.Count)
            {
                Console.WriteLine($"Node {Identifier} is leader for term {term}");
                CurrentRole = Role.LEADER;
                HeartbeatTimer.Start();
                ElectionCountdownTimer.Stop();
                return voteCount;
            }
            else
            {
                Thread.Sleep(WaitTime * 2);
                if (CurrentRole != Role.FOLLOWER)
                {
                    await StartElectionAsync(term++);
                }
                else
                {

                    ElectionCountdownTimer.Start();
                }

            }

            return 0;
        }

        public void AddToLogAsFollower(string key, string value, int logIndex)
        {
            if (!keyValueLog.ContainsKey(key))
            {
                keyValueLog[key] = (value, logIndex);
                Console.WriteLine($"Node {Identifier} as {CurrentRole} stored the value");
            }
        }



        public (string, int) EventualGet(string key)
        {
            // Return the latest value from the log
            if (keyValueLog.ContainsKey(key))
            {
                var value = keyValueLog[key];
                var item = (value.Item1, value.Item2);
                Console.WriteLine($"Eventual get: {item}");
                return item;
            }
            else
            {
                return ("0", 0); // Key not found
            }
        }

        public async Task<bool> CompareVersionAndSwap(string key, string newValue, int expectedVersion)
        {//This takes an existing key and sets a new value to it. Should only work on leader. Compare logic when that happens
         //Should also trigger a wave of updates
         //DO THIS
            if (keyValueLog.TryGetValue(key, out (string, int) value))
            {
                var (_, version) = value;
                if (version == expectedVersion)
                {
                    Console.WriteLine($"Updating {key} to {newValue}");
                    keyValueLog[key] = (newValue, version);

                    if (CurrentRole == Role.LEADER)
                    {
                        foreach (var item in List)
                        {
                            if (item.Key.ToString() != Identifier)
                            {
                                HttpClient client = clientMaker(item.Key.ToString());
                                await client.PostAsync($"CompareVersionAndSwap/{key}/{newValue}/{expectedVersion}", null);
                            }
                        }
                    }

                    return true; // Successful swap
                }
                else
                {
                    return false; // Version mismatch
                }
            }
            else
            {
                return false; // Key not found
            }
        }
        public async Task<(string, int)> StrongGet(string key)
        {
            // Check if this node is the current leader
            if (CurrentRole == Role.LEADER)
            {
                bool result = await ActuallyLeader();
                if (result)
                {
                    var item = EventualGet(key);
                    Console.WriteLine(item);
                    return item;
                }
            }
            return ("0", 0);
        }

        public async Task<bool> ActuallyLeader()
        {
            int count = 0;

            foreach (var node in List)
            {
                HttpClient client = clientMaker(node.Key.ToString());
                bool isLeader = await client.GetFromJsonAsync<bool>($"Node/CheckIsLeader/{Identifier}");
                if (isLeader)
                {
                    count++;
                }
            }
            if (count * 2 > List.Count)
            {
                return true;
            }
            return false;
        }

        public bool CheckIsLeader(string identifier)
        {
            var subjectNode = FindLeader();
            if (subjectNode == identifier)
            {
                return true;
            }
            return false;
        }

        public async Task AddToLogAsLeaderAsync(string key, string value)
        {//DO THIS
            if (!keyValueLog.ContainsKey(key))
            {

                Console.WriteLine($"Adding value {value} to {key}");
                keyValueLog[key] = (value, keyValueLog.Values.Count() + 1);

                foreach (var item in List)
                {
                    if (item.Key.ToString() != Identifier)
                    {
                        HttpClient client = clientMaker(item.Key.ToString());
                        await client.PostAsync($"Node/AddToLogFollower/{key}/{value}/{keyValueLog.Values.Count() + 1}", null);
                    }
                }
            }
            // CreateLogFile();
        }
        // private void CreateLogFile()
        // {
        //     string filePath = Path.Combine("information", $"{Identifier}Info.txt");
        //     if (!File.Exists(filePath))
        //     {
        //         File.Create(filePath).Close();
        //     }
        //     else
        //     {
        //         // Clear file contents
        //         File.WriteAllText(filePath, string.Empty);
        //     }

        //     using (StreamWriter writer = new StreamWriter(filePath))
        //     {

        //         var sortedDict = keyValueLog.OrderBy(kv => kv.Value.Item2);

        //         foreach (var kvp in sortedDict)
        //         {
        //             // Format: key, value1
        //             string line = $"{kvp.Key},{kvp.Value.Item1}";
        //             writer.WriteLine(line);
        //         }
        //     }
        // }
        // private void ReadInLogFile()
        // {
        //     string filePath = Path.Combine("information", $"{Identifier}Info.txt");
        //     Dictionary<string, (string, int)> dictionary = [];

        //     // Check if the file exists
        //     if (File.Exists(filePath))
        //     {
        //         // Read each line from the file

        //         int lineNum = 1;
        //         foreach (string line in File.ReadLines(filePath))
        //         {
        //             if (!string.IsNullOrWhiteSpace(line))
        //             {
        //                 // Split the line by comma
        //                 string[] parts = line.Split(',');

        //                 if (parts.Length == 3)
        //                 {
        //                     string key = parts[0];
        //                     string value1 = parts[1];
        //                     // Add to the dictionary
        //                     dictionary[key] = (value1, lineNum);
        //                 }
        //                 else
        //                 {
        //                     // Handle invalid format
        //                     Console.WriteLine($"Invalid line format: {line}");
        //                 }
        //             }
        //             else
        //             {
        //                 // Handle empty line
        //                 Console.WriteLine("Empty line encountered.");
        //             }

        //             lineNum++;
        //         }
        //     }
        //     else
        //     {
        //         Console.WriteLine("File does not exist.");

        //     }
        //     keyValueLog = dictionary;
        // }

    }





}
public enum Role
{
    FOLLOWER,
    CANDIDATE,
    LEADER,
}


