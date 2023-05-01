using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Models;

using Microsoft.AspNetCore.Mvc;

namespace iSportsRecruiting.Server.Controllers.v1
{
    public class CoachController : BaseApiController<CoachController>
    {

        [HttpGet("sport/{sportId:int}")]
        public async Task<ActionResult> Get(int sportId)
        {
            try
            {
                var coachs = await _context.GetCoachsBySportIdAsync(sportId);

                return Ok(new Response<IEnumerable<CoachModel>>(coachs));
            }
            catch (Exception e)
            {
                return Ok(new Response<IEnumerable<CoachModel>> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("sport/{sportId:int}/{universityId:int}")]
        public async Task<ActionResult> GetByUniversity(int sportId, int universityId)
        {
            try
            {
                var coach = await _context.GetCoachsByUniversityIdAsync(sportId, universityId);

                return Ok(new Response<CoachModel>(coach));
            }
            catch (Exception e)
            {
                return Ok(new Response<CoachModel> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostCoach(SportContactDTO coach)
        {
            try
            {
                var id = await _context.AddCoachAsync(coach);

                return Ok(new Response<int>(id));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateCoach(SportContactDTO coach)
        {
            try
            {
                await _context.UpdateCoachAsync(coach);

                return Ok(new Response<SportContactDTO>(coach));
            }
            catch (Exception e)
            {
                return Ok(new Response<SportContactDTO> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteCoach(int id)
        {
            try
            {
                var result = await _context.DeleteCoachAsync(id);

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("views/{athleteId:int}")]
        public async Task<ActionResult> GetViews(int athleteId)
        {
            try
            {
                var views = await _context.GetCoachViewsAsync(athleteId);

                return Ok(new Response<IEnumerable<CoachViewModel>>(views));
            }
            catch (Exception e)
            {
                return Ok(new Response<IEnumerable<CoachViewModel>> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }
        [HttpGet("views/mobile/{athleteId:int}")]
        public async Task<ActionResult> GetViewsMobile(int athleteId)
        {
            try
            {
                var views = await _context.GetCoachViewsAsync(athleteId);
                var groupedViews = views?.GroupBy(c => c.Coach_Id).Select(c =>
                {
                    var view = c.OrderByDescending(a => a.Id).First();
                    view.Count = c.Count();
                    return view;
                }).ToList();

                return Ok(new Response<IEnumerable<CoachViewModel>>(groupedViews));
            }
            catch (Exception e)
            {
                return Ok(new Response<IEnumerable<CoachViewModel>> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }


        [HttpGet("views/{athleteId:int}/count")]
        public async Task<ActionResult> GetViewsCount(int athleteId)
        {
            try
            {
                var views = await _context.GetCoachViewsCountAsync(athleteId);

                return Ok(new Response<int>(views));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPost("views")]
        public async Task<ActionResult> Post(CoachViewModel model)
        {
            try
            {
                var result = await _context.AddCoachViewsAsync(model);

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("contact/{universityId:int}/{sport}")]
        public async Task<ActionResult> GetUniversityContact(int universityId, string sport)
        {
            try
            {
                var result = await _context.GetSportsContactByUniversityAndSportAsync(universityId, sport);
                return Ok(new Response<SportContactModel>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response<SportContactModel> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetCoachByContactAsync(int id)
        {
            try
            {
                var result = await _context.GetCoachByIdAsync(id);
                return Ok(new Response<CoachModel>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response<CoachModel> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }
    }
}
