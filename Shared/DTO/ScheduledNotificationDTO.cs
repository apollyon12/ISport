
namespace iSportsRecruiting.Shared.DTO
{
    public class ScheduledNotificationDTO
    {
        public int Id { get; set; }
        public string DayOfTheWeek { get; set; }
        public int? DayOfTheMonth { get; set; }
        public bool Daily { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
        public bool Send_Email { get; set; }
        public bool Send_Sms { get; set; }
        public bool Is_Deleted { get; set; }

    }
}
