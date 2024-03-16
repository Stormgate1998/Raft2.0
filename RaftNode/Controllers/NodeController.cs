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

    [HttpPost("AddToLog/{key}/{value}")]
    public void AddToLog(string key, string value)
    {
        node.AddToLogAsLeaderAsync(key, value);
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
    public async Task<string?> StrongGetAsync(string key)
    {
        return await node.StrongGet(key);
    }


    [HttpGet("EventualGet/{key}")]
    public string? EventualGetAsync(string key)
    {
        return node.EventualGet(key);
    }

    [HttpPost("CompareVersionAndSwap/{key}/{newValue}/{expectedVersion}")]
    public async Task<bool> CompareVersionAndSwap(string key, string newValue, int expectedVersion)
    {
        return await node.CompareVersionAndSwap(key, newValue, expectedVersion);
    }

}

