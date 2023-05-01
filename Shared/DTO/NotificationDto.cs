namespace iSportsRecruiting.Shared.DTO;

public class NotificationDto
{
    public string Url { get; set; }
    public string P256dh { get; set; }
    public string Auth { get; set; }
    
    //Custom
    public string Message { get; set; }
    public string RedirectTo { get; set; }
    public string Icon { get; set; }
}