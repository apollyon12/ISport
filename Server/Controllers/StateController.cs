using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Services;
using System;

namespace iSportsRecruiting.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StateController : ControllerBase
    {
        private readonly IDatabaseContext _context;
        private readonly ILogger<StateController> _logger;

        public StateController(IDatabaseContext context, ILogger<StateController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> Get(int? countryId = 231, string search = null)
        {
            try
            {
                var response = new Response<IEnumerable<StateDTO>>();

                var states = await _context.GetStatesAsync(countryId, search);

                response.Payload = states.Select(s => s.ToDTO());

                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return Problem();
        }
    }
}
