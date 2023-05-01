namespace iSportsRecruiting.Shared.DTO
{
    public class ContactDTO
    {
        public int Id { get; set; }
        public bool IsSelected { get; set; }
        public bool IsLead { get; set; }
        public string Image { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Sport { get; set; }
        public string Role { get; set; }
    }

    public class MessageSmsDTO
    {
        public string From { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string Sms { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public DateTime? Date { get; set; }
    }

    public class SmsParentDTO
    {
        public ContactDTO[] Contacts { get; set; }
        public string Message { get; set; }
    }

    public class EmailParentDTO
    {
        public ContactDTO[] Contacts { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
    }
}
