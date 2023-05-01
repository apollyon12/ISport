using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;

using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Server.Controllers.v1
{
    public class LeadController : BaseApiController<LeadController>
    {
        [HttpGet("{userId:int?}")]
        public async Task<ActionResult> Get(int? userId = null)
        {
            try
            {
                var models = await _context.GetLeadsAsync(userId);
                var dtos = models.Select(m => m.ToDTO());

                return Ok(new Response<IEnumerable<LeadDTO>>(dtos));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }
        [HttpGet("get/{id:int}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                var model = await _context.GetLeadByIdAsync(id);
                var dto = new LeadDTO
                {
                    Id = model.Id,
                    UserId = model.User_Id,
                    AthleteId = model.Athlete_Id,
                    FullName = model.Full_Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    Sport = model.Sport,
                    GraduationYear = model.Graduation_Year,
                    AddedOn = model.Added_On,
                    Status = model.Status,
                    SocialMedia = model.Social_Media,
                    Notes = model.Notes,
                    GPA = model.GPA
                };

                return Ok(new Response<LeadDTO>(dto));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("check/{email}")]
        public async Task<ActionResult> CheckIfEmailExists(string email)
        {
            try
            {
                var result = await _context.CheckIfLeadExists(email);

                return Ok(new Response<bool>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response<bool> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("{userId:int}/count")]
        public async Task<ActionResult> GetCount(int userId)
        {
            try
            {
                var count = await _context.GetLeadsCountAsync(userId);

                return Ok(new Response<int>(count));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPut]
        public async Task<ActionResult> Put(LeadDTO dto)
        {
            try
            {
                var result = await _context.UpdateLeadAsync(dto.ToModel());

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(LeadDTO dto)
        {
            try
            {
                var result = await _context.AddLeadAsync(dto.ToModel());

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var result = await _context.DeleteLeadAsync(id);

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }
    }
}
