using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace iSportsRecruiting.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DivisionController : ControllerBase
    {
        private readonly IDatabaseContext _context;
        private readonly ILogger<DivisionController> _logger;

        public DivisionController(IDatabaseContext context, ILogger<DivisionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var divisions = await _context.GetDivisionsAsync();
                return Ok(new Response<IEnumerable<DivisionDTO>>
                {
                    Payload = divisions.Select(d => d.ToDTO())
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return Problem();
        }
    }
}
