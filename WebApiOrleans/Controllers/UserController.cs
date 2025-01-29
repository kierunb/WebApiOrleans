using Microsoft.AspNetCore.Mvc;
using Orleans.Runtime;
using WebApiOrleans.Grains;

namespace WebApiOrleans.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IGrainFactory _grainFactory;
    private readonly IClusterClient _clusterClient;

    public UserController(
        IGrainFactory grainFactory,
        IClusterClient clusterClient)
    {
        _grainFactory = grainFactory;
        _clusterClient = clusterClient;

        // IClusterClient is meant to be used from the client which is accessing the silo cluster (ex. webapi controller).
        // IGrainFactory is used in a service class within a silo to get an instance of a Grain.
        // https://stackoverflow.com/questions/60407743/in-orleans-when-to-use-igrainfactory-vs-iclusterclient
    }


    [HttpGet("set/{id:long}/{firstname}/{lastname}")]
    public async Task<IActionResult> SetUserData(long id, string firstname, string lastname)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(id);
        await userGrain.SetUserData(firstname, lastname);

        return Ok();
    }

    [HttpGet("get/{id:long}")]
    public async Task<IActionResult> GetUserData(long id)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(id);
        var userData = await userGrain.GetUserData();

        return Ok(userData);
    }

    [HttpGet("say-hello/{message}")]
    public async Task<IActionResult> HelloGrain(string message)
    {
        var grain = _grainFactory.GetGrain<IHelloGrain>("hello-grain-1");
        await grain.SayHello(message);
        return Ok();
    }

    [HttpGet("say-hello-user/{message}/{userId:int}")]
    public async Task<IActionResult> HelloUserGrain(string message, int userId)
    {
        var grain = _grainFactory.GetGrain<IHelloGrain>("hello-grain-1");
        await grain.SayHelloUser(message, userId);
        return Ok();
    }
}
