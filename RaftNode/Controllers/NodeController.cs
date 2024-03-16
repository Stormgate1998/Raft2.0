using Microsoft.AspNetCore.Mvc;
using RaftNode.Services;
namespace RaftNode.Controllers;

[ApiController]
[Route("[controller]")]
public class NodeController : Controller
{
    private readonly NodeService node;
    private readonly int delaySeconds;

    public NodeController(NodeService node)
    {
        this.node = node;
    }

    [HttpGet("GetId")]
    public string GetId()
    {
        return node.ReturnId();
    }

    [HttpGet("GetOtherId/{id}")]
    public async Task<string> GoGetCopyOfImage(int id)
    {
        return await node.ReturnOtherId(id.ToString());
    }

    [HttpGet("GetNodeList")]
    public Dictionary<int, string> GetNodeList()
    {
        return node.ReturnList();
    }

    [HttpGet("CheckIsLeader/{id}")]
    public bool IsLeader(int id)
    {
        return node.CheckIsLeader(id.ToString());
    }

    [HttpGet("ProcessVoteRequest/{id}/{term}")]
    public bool? ProcessVoteRequest(int id, int term)
    {
        return node.ProcessVoteRequest(id.ToString(), term);
    }

    [HttpPost("AddToLogFollower/{key}/{value}/{logIndex}")]
    public void AddToLogAsFollower(string key, string value, int logIndex)
    {
        node.AddToLogAsFollower(key, value, logIndex);
    }

    [HttpPost("AddToLog")]
    public void AddToLog(LogObject logObject)
    {
        node.AddToLogAsLeaderAsync(logObject.key, logObject.value);
    }


    [HttpPost("ProcessHeartBeat/{id}/{term}")]
    public void ProcessHeartBeat(int id, int term)
    {
        node.ProcessHeartbeat(id.ToString(), term);
    }

    [HttpGet("FindLeader")]
    public string FindLeader()
    {
        return node.FindLeader();
    }

    [HttpGet("StrongGet/{key}")]
    public async Task<ActionResult<Item>> StrongGetAsync(string key)
    {
        var (item, item2) = await node.StrongGet(key);
        Console.WriteLine($"http {item}, {item2}");
        Item item1 = new(item, item2);
        Console.WriteLine($"Item {item1.value}, {item1.log}");
        return Ok(item1);

    }


    [HttpGet("EventualGet/{key}")]
    public ActionResult<Item> EventualGet(string key)
    {
        var (item, item2) = node.EventualGet(key);
        Console.WriteLine($"http {item}, {item2}");
        Item item1 = new(item, item2);
        Console.WriteLine(item1.ToString());
        return Ok(item1);
    }

    [HttpPost("CompareVersionAndSwap/{key}/{newValue}/{expectedVersion}")]
    public async Task<bool> CompareVersionAndSwap(string key, string newValue, int expectedVersion)
    {
        return await node.CompareVersionAndSwap(key, newValue, expectedVersion);
    }

}
public class Item(string value, int log)
{
    public string value = value;
    public int log = log;
}
