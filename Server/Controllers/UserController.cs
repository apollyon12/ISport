using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Models;
using iSportsRecruiting.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSportsRecruiting.Server.Controllers
{
    public class UserController : BaseApiController<UserController>
    {
        [HttpGet]
        public async Task<ActionResult> Get(int? id = null, string search = null, int? page = null, int? take = null)
        {
            var response = new Response<IEnumerable<UserDTO>>();

            try
            {
                if (!(page.HasValue && take.HasValue))
                {
                    var users = await _context.GetUsersAsync(id);

                    response.Payload = users.Select(u => u.ToDTO());

                    return Ok(response);
                }
                else
                {
                    var users = await _context.GetUsersAsync(pagination: (search, (page.Value - 1) * take.Value, take.Value));

                    response.Payload = users.Select(u => u.ToDTO());

                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet]
        [Route("count")]
        public async Task<ActionResult> Count()
        {
            try
            {
                var count = await _context.GetUsersCountAsync();

                return Ok(new Response<int>
                {
                    Payload = count
                });
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }
    }
}
