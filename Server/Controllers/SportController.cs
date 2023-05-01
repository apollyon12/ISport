using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iSportsRecruiting.Server.Controllers
{
    [AllowAnonymous]
    public class SportController : BaseApiController<SportController>
    {
        [HttpGet]
        public async Task<ActionResult> Get(int? id = null, string? search = "", int? page = null, int? take = null)
        {
            var response = new Response<IEnumerable<SportDTO>>();

            try
            {
                if (id is not null)
                {
                    var sports = await _context.GetSportsAsync(id);

                    response.Payload = sports.Select(s => s.ToDTO());
                }
                else if (!(page.HasValue && take.HasValue))
                {
                    var sports = await _context.GetSportsAsync();

                    response.Payload = sports.Select(s => s.ToDTO());
                }
                else
                {
                    var count = await _context.GetSportsCountAsync();
                    var sports = await _context.GetSportsAsync(pagination: (search, 0, take.Value));

                    response.Total = count;
                    response.Payload = sports.Select(s => s.ToDTO());
                }

                return Ok(response);
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpGet]
        [Route("contact/{id:int}/{isSelfId:bool}")]
        public async Task<ActionResult> Get(int id, bool isSelfId)
        {
            try
            {
                IEnumerable<SportContactModel> sports;

                if (isSelfId)
                    sports = await _context.GetSportsContactByUniversityAsync(sportContactId: id);
                else
                    sports = await _context.GetSportsContactByUniversityAsync(id);

                return Ok(new Response<IEnumerable<SportContactDTO>>
                {
                    Payload = sports.Select(s => s.ToDTO())
                });
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _context.RemoveSportAsync(id);

            return Ok();
        }

        [HttpPut]
        [Route("contact")]
        public async Task<ActionResult> Put(SportContactDTO sport)
        {
            try
            {
                var result = await _context.UpdateSportContactAsync(sport.ToModel());

                if (result == 1)
                    return Ok(new Response());
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpPost]
        [Route("contact")]
        public async Task<ActionResult> Post(SportContactDTO sport)
        {
            try
            {
                var result = await _context.PostSportContactAsync(sport.ToModel());

                if (result == 1)
                    return Ok(new Response());
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

        [HttpDelete]
        [Route("contact/{id:int}")]
        public async Task<ActionResult> DeleteSportContact(int id)
        {
            try
            {
                var result = await _context.DeleteSportContactAsync(id);

                if (result == 1)
                    return Ok(new Response());
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
                var count = await _context.GetSportsCountAsync();

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

        [HttpGet]
        [Route("{id:int}/position")]
        public async Task<ActionResult> GetPositionBySport(int id)
        {
            try
            {
                var positions = await _context.GetPositionsBySport(id);

                return Ok(new Response<IEnumerable<PositionModel>>
                {
                    Payload = positions
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
