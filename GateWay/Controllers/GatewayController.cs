using GateWay.Services;
using Microsoft.AspNetCore.Mvc;
namespace GateWay.Controllers;

[ApiController]
[Route("[controller]")]
public class GatewayController : Controller
{
    private readonly Gateway gateway;
    private readonly int delaySeconds;

    public GatewayController(Gateway gateway)
    {
        this.gateway = gateway;
    }

    [HttpGet("GetIdFromNode/{id}")]
    public async Task<string> GetId(int id)
    {
        return await gateway.ReturnIdOfNodeAsync(id.ToString());
    }


    [HttpGet("GetSecondId/{idone}/{idtwo}")]
    public async Task<string> GoGetCopyOfImage(int idone, int idtwo)
    {
        return await gateway.ReturnIdFromSecondNode(idone.ToString(), idtwo.ToString());
    }

    [HttpGet("GetNodeList")]
    public List<string> GetNodeList()
    {
        return gateway.ReturnList();
    }
}

