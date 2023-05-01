using System;

namespace iSportsRecruiting.Shared.Models
{
    public class ConfirmEmail
    {
        public int Id { get; set; }
        public int University_Id { get; set; }
        public int Contact_Id { get; set; }
        public bool Is_Coach { get; set; }
        public int Sport_Id { get; set; }
        public string Cipher { get; set; }
        public string Full_Name { get; set; }
        public string Email { get; set; }

        public string Token { get; set; } = Guid.NewGuid().ToString();
        public long Date_Ticks { get; set; } = DateTime.Now.AddDays(1).Ticks;
    }
}
