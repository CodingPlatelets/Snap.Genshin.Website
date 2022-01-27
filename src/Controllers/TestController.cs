﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Snap.Genshin.Website.Services;
#if DEBUG
namespace Snap.Genshin.Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public TestController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider serviceProvider;

        [HttpGet("RefreshStatistics")]
        public async Task<IActionResult> RefreshStatistics()
        {
            var service = serviceProvider.GetRequiredService<GenshinStatisticsService>();
            await service.CaltulateStatistics();
            return Ok();
        }
    }
}
#endif