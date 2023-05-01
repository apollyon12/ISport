using System;
using System.Collections.Generic;
using iSportsRecruiting.Shared.Models;

namespace iSportsRecruiting.Shared.Email
{
    public class EmailTempData
    {
        public List<EmailModel> History { get; set; } = new();
        public EmailModel ReplyTo { get; set; }
        public string Email { get; set; }
    }
}
