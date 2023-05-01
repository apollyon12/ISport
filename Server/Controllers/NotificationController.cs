using iSportsRecruiting.Client.Pages.Admin;
using iSportsRecruiting.Shared.DTO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebPush;

namespace iSportsRecruiting.Server.Controllers;

public class NotificationController : BaseApiController<NotificationController>
{
    [HttpPost("send")]
    public async Task<ActionResult> SendAsync(NotificationDto notification)
    {
        try
        {
            await SendNotificationAsync(notification);
            return Ok(new Response {Status = ResponseStatus.Ok});
        }
        catch (Exception e)
        {
            return Ok(new Response {Status = ResponseStatus.InternalError, Message = e.Message});
        }
    }
    [HttpPost("send/schedulednotification")]
    public async Task<ActionResult> SendScheduledNotificationAsync(ScheduledNotificationDTO notification)
    {
        try
        {
            var result = await _context.AddScheduledNotificationAsync(notification);

            return Ok(new Response<int>(result));
        }
        catch (Exception e)
        {
            return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
        }
    }
    [HttpGet("schedulednotification")]
    public async Task<ActionResult> GetScheduledNotification()
    {
        try
        {
            var response = await _context.GetScheduledNotificationAsync();
            var responseToDto = response.Select(a => a.ToDTO());

            return Ok(new Response<IEnumerable<ScheduledNotificationDTO>>(responseToDto));
        }
        catch (Exception e)
        {
            return Ok(new Response { Status = ResponseStatus.InternalError, Message = e.Message });
        }
    }
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteScheduledNotification(int id)
    {
        try
        {
            var result = await _context.DeleteScheduledNotificationAsync(id);

            return Ok(new Response<int>(result));
        }
        catch (Exception e)
        {
            return Ok(new Response<int> { Status = ResponseStatus.InternalError, Message = e.Message });
        }
    }


    private static async Task SendNotificationAsync(NotificationDto subscription)
    {
        var publicKey = "BKxn0bNMRIXloqviU58PM7MzXj4EKp_EgBkF67bUZokq8azmyMptPb161QvhbPbEZglFvty2nKHCMBabJ3Qf-oA";
        var privateKey = "h9nBja-IVNuD7qWSOyR6NWQA_74Ru6cRoGtMcqWubqE";

        var pushSubscription = new PushSubscription(subscription.Url, subscription.P256dh, subscription.Auth);
        var vapidDetails = new VapidDetails("mailto: <info@isportsrecruiting.com>", publicKey, privateKey);
        var webPushClient = new WebPushClient();
        try
        {
            string payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                message = subscription.Message,
                url = subscription.RedirectTo,
                icon = subscription.Icon
            });
            await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync("Error sending push notification: " + ex.Message);
        }
    }
}