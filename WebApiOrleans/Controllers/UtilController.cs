using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using WebApiOrleans.Grains;

namespace WebApiOrleans.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilController : ControllerBase
    {
        private readonly IGrainFactory _grainFactory;

        public UtilController(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
        }

        [HttpGet("timer")]
        public async Task<IActionResult> StartTimer()
        {
            var grain = _grainFactory.GetGrain<ITimerGrain>(Guid.NewGuid());
            grain.StartTimer();

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
}
