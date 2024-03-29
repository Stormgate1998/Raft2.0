﻿using GateWay.Services;
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

    [HttpGet("GetLeader")]
    public async Task<string> GetLeader()
    {
        bool valid = await gateway.IsLeaderValid();
        if (valid)
        {
            return gateway.Leader;
        }
        else
        {

            await gateway.FindLeader();
            return gateway.Leader;
        }
    }


    [HttpGet("GetSecondId/{idone}/{idtwo}")]
    public async Task<string> GoGetCopyOfImage(int idone, int idtwo)
    {
        return await gateway.ReturnIdFromSecondNode(idone.ToString(), idtwo.ToString());
    }

    [HttpGet("GetNodeList")]
    public Dictionary<int, string> GetNodeList()
    {
        return gateway.ReturnList();
    }

    [HttpPost("AddToLog/{key}/{value}")]
    public async Task AddToLog(string key, string value)
    {
        LogObject logObject = new(key, value);
        await gateway.AddToLog(logObject);
    }

    [HttpGet("StrongGet/{key}")]
    public async Task<string> StrongGetAsync(string key)
    {
        return await gateway.StrongGet(key);
    }


    [HttpGet("EventualGet/{key}")]
    public async Task<string> EventualGetAsync(string key)
    {
        return await gateway.EventualGet(key);
    }

    [HttpPost("CompareVersionAndSwap/{key}/{newValue}/{expectedVersion}")]
    public async Task<bool> CompareVersionAndSwap(string key, string newValue, int expectedVersion)
    {
        return await gateway.CompareVersionAndSwap(key, newValue, expectedVersion);
    }
}
