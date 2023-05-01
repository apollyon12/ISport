using System.Collections.Generic;

namespace iSportsRecruiting.Shared.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public bool IsConversationObtained { get; set; }
        public bool IsOnline { get; set; }
        public bool IsSelected { get; set; }
        public bool IsWriting { get; set; }
        public int MessagesCount { get; set; }
        public List<EmailModel> Conversation { get; set; } = new List<EmailModel>();

        public string Style => IsSelected ? "background-color: #ebebeb;" : "";
    }
}
