using iSportsRecruiting.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iSportsRecruiting.Server.Controllers.v1
{
    public class AssistantController : BaseApiController<AssistantController>
    {
        [HttpGet("{id?}")]
        public async Task<ActionResult> Obtain(int? id = null)
        {
            try
            {
                var models = await _context.GetAssistantsAsync(id);
                var dtos = models.Select(m => m.ToDTO());

                return Ok(new Response<IEnumerable<AssistantDTO>>(dtos));
            }
            catch(Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult> Count()
        {
            try
            {
                var assistants = await _context.GetAssistantsCountAsync();

                return Ok(new Response<int>(assistants));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, AssistantDTO dto)
        {
            try
            {
                var result = await _context.UpdateAssistantAsync(dto.ToModel());

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Add(AssistantDTO dto)
        {
            try
            {
                var result = await _context.AddAssistantAsync(dto.ToModel());

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Remove(int id, [FromQuery]string email)
        {
            try
            {
                var result = await _context.DeleteAssistantAsync(id, email);

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

    }
}
