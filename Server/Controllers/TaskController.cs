using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;

using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Server.Controllers.v1
{
    public class TaskController : BaseApiController<TaskController>
    {
        [HttpGet("{userId:int}")]
        public async Task<ActionResult> Get(int userId)
        {
            try
            {
                var models = await _context.GetTasksAsync(userId);
                var dtos = models.Select(m => m.ToDTO());

                return Ok(new Response<IEnumerable<TaskDTO>>(dtos));
            }
            catch (Exception e)
            {
                return Ok(new Response<IEnumerable<TaskDTO>> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("{userId:int}/count")]
        public async Task<ActionResult> GetCount(int userId)
        {
            try
            {
                var tasks = await _context.GetTasksCountAsync(userId);

                return Ok(new Response<int>(tasks));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPut]
        public async Task<ActionResult> Put(TaskDTO dto)
        {
            try
            {
                var result = await _context.UpdateTaskAsync(dto.ToModel());

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(TaskDTO dto)
        {
            try
            {
                var result = await _context.AddTaskAsync(dto.ToModel());

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _context.DeleteTaskAsync(id);

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }
    }
}
