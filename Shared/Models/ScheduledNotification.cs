using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class ScheduledNotification
    {
        public int Id { get; set; }
        public string DayOfTheWeek { get; set; }
        public int DayOfTheMonth { get; set; }
        public bool Daily { get; set; }
        public string Entities { get; set; }
        public string Content { get; set; }
        public bool Send_Email { get; set; }
        public bool Send_Sms { get; set; }
        public bool Is_Deleted { get; set; }

        public ScheduledNotificationDTO ToDTO()
        {
            return new ScheduledNotificationDTO
            {
                Id = Id,
                Daily = Daily,
                To = Entities,
                Message = Content,
                Send_Email = Send_Email,
                Send_Sms = Send_Sms,
                DayOfTheMonth = DayOfTheMonth,
                DayOfTheWeek = DayOfTheWeek,
                Is_Deleted = Is_Deleted
            };
        }
    }
    

}
