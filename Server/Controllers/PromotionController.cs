using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;

using iSportsRecruiting.Shared.DTO;
using iSportsRecruiting.Shared.Models;

namespace iSportsRecruiting.Server.Controllers.v1
{
    public class PromotionController : BaseApiController<PromotionController>
    {
        [HttpGet("{id?}")]
        public async Task<ActionResult> Get(int? id = null)
        {
            try
            {
                var models = await _context.GetPromotionsAsync(id);

                return Ok(new Response<IEnumerable<PromotionModel>>(models));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("count")]
        public async Task<ActionResult> Count()
        {
            try
            {
                var promotions = await _context.GetPromotionsCountAsync();

                return Ok(new Response<int>(promotions));
            }
            catch (Exception e)
            {
                return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult> GetByCode(string code)
        {
            try
            {
                var models = await _context.GetPromotionByCodeAsync(code);

                return Ok(new Response<PromotionModel>(models));
            }
            catch (Exception e)
            {
                return Ok(new Response<PromotionModel> { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPut]
        public async Task<ActionResult> Put(PromotionModel promotion)
        {
            try
            {
                var result = await _context.UpdatePromotionAsync(promotion);

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(PromotionModel promotion)
        {
            try
            {
                var result = await _context.AddPromotionAsync(promotion);

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
                var result = await _context.DeletePromotionAsync(id);

                return Ok(new Response<int>(result));
            }
            catch (Exception e)
            {
                return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
            }
        }
    }
}
