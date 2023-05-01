using Microsoft.AspNetCore.Mvc;

using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Server.Controllers.v1
{
    public class HubController : BaseApiController<HubController>
    {
        [HttpPost("user")]
        public ActionResult AddUserAsync(HubUser user)
        {
            //SignalRHub.Users.Add(user);
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> AddEmailNotification(Notification notification)
        {
            try
            {
                var notificationId = await _context.AddNotificationAsync(notification);

                notification.Id = notificationId;

                //var connectionId = SignalRHub.GetConnectionIdByUserId(notification.Entity_Id);
                //_ = _hubContext.Clients.Client(connectionId).SendAsync("Notifications", notification);

                return Ok(new Response<int>(notificationId));
            }
            catch
            {
                return Problem();
            }
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult> GetNotificationsAsync(int userId)
        {
            try
            {
                var notifications = await _context.GetNotificationByUserIdAsync(userId);

                return Ok(new Response<IEnumerable<Notification>> { Payload = notifications });
            }
            catch
            {
                return Problem();
            }
        }

        [HttpDelete("{senderId:int}/{receiverId:int}")]
        public async Task<ActionResult> DeleteNotificationAsync(int senderId, int receiverId)
        {
            try
            {
                var notifications = await _context.DeleteNotificationAsync(senderId, receiverId);

                return Ok();
            }
            catch
            {
                return Problem();
            }
        }
    }
}
