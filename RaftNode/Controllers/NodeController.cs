using Microsoft.AspNetCore.Mvc;
using RaftNode.Services;
namespace RaftNode.Controllers;

[ApiController]
[Route("[controller]")]
public class NodeController : Controller
{
    private readonly Node node;
    private readonly int delaySeconds;

    public NodeController(Node node)
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
    public List<string> GetNodeList()
    {
        return node.ReturnList();
    }
}

