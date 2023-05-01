using System;

namespace iSportsRecruiting.Shared.Models
{
    public class EmailModel
    {
        public int Id { get; set; }
        public int Sender_Id { get; set; }
        public string Sender_Name { get; set; }
        public string Sender_Image { get; set; }
        public int Receiver_Id { get; set; }
        public string Receiver_Name { get; set; }
        public string Receiver_Image { get; set; }
        public int Receiver_Type { get; set; }
        public int University_Id { get; set; }
        public string University_Name { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public int Replied_To { get; set; }
        public long TimeStamp { get; set; } = DateTime.Now.ToUniversalTime().Ticks;
        public DateTime Date { get; set; } = DateTime.Now.ToUniversalTime();
        public int Replies { get; set; }
        public bool Sent { get; set; } = true;
        public bool Error { get; set; }
        public bool Readed { get; set; }
        public bool Is_Deleted { get; set; }
    }
}
