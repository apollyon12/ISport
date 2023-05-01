using System;
using System.Collections.Generic;
using System.Text;

namespace iSportsRecruiting.Shared.DTO
{
    public class AccountDto
    {
        public int Id { get; set; }
        public UserType Type { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public UserDTO Admin { get; set; }
        public AthleteDTO Athlete { get; set; }
        public CoachDTO Coach { get; set; }
        public AssistantDTO Assistant { get; set; }
        public List<Notification> Notifications { get; set; } = new();

        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public bool ShouldResetPassword { get; set; }
    }

    public enum UserType
    {
        None,
        Admin,
        AssisAdmin,
        AssisCoord,
        Athlete,
        Coach
    }

    public class AthleteRegisterDto
    {
        public AthleteDTO? Info { get; set; }
        public CreditCardTransactionDTO? CreditCardTransaction { get; set; }
        public string? Cipher { get; set; }
    }

    public class Notification
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public int Entity_Id { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Link { get; set; }
        public string Type { get; set; }
        public string Image { get; set; }
        public int External_Relate_Id { get; set; }
        public int Count { get; set; }

        public void SetIcon(string icon)
        {
            Icon = Convert.ToBase64String(Encoding.Default.GetBytes(icon));
        }

        public string GetIcon()
        {
            return Encoding.Default.GetString(Convert.FromBase64String(Icon));
        }
    }
}
