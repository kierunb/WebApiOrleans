using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiOrleans.Grains;

namespace WebApiOrleans.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IGrainFactory _grainFactory;

        public UserController(IGrainFactory grainFactory)
        {
            _grainFactory = grainFactory;
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

    }
}
