using iSportsRecruiting.Server.Controllers.v1;
using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace iSportsRecruiting.Server.Controllers
{
    public class SocialMediaController : BaseApiController<SocialMediaController>
    {
        [HttpGet("{userId:int}")]
        public async Task<ActionResult> GetByUser(int userId)
        {
            try
            {
                var socialMedia = await _context.GetSocialMediaByUserIdAsync(userId);

                return Ok(new Response<SocialMediaModel>(socialMedia));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(SocialMediaModel socialMedia)
        {
            try
            {
                var id = await _context.AddSocialMediaAsync(socialMedia);
                return Ok(new Response<int>(id));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Message = e.Message, Status = ResponseStatus.InternalError });
            }
        }
        [HttpPut]
        public async Task<ActionResult> Put(SocialMediaDTO socialMedia)
        {
            try
            {
                var result = await _context.UpdateSocialMediaAsync(socialMedia.ToModel());

                if (result == 1)
                    return Ok(new Response());
            }
            catch (Exception e)
            {
                //_logger.LogError(e.Message);
            }

            return Problem();
        }

    }
}
