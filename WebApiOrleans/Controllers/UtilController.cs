using Microsoft.AspNetCore.Mvc;
using WebApiOrleans.Grains;

namespace WebApiOrleans.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UtilController : ControllerBase
{
    private readonly IGrainFactory _grainFactory;

    public UtilController(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    [HttpGet("ping-grain")]
    public async Task<IActionResult> PingGrain()
    {
        var grain = _grainFactory.GetGrain<IPingGrain>(Guid.NewGuid());
        await grain.Ping();

        return Ok();
    }

    [HttpGet("reminder")]
    public async Task<IActionResult> StartReminder()
    {
        var grain = _grainFactory.GetGrain<IReminderGrain>(Guid.NewGuid().ToString());
        await grain.SendMessage();

        return Ok();
    }
}
