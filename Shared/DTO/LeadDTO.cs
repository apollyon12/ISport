using System;
using System.Globalization;
using iSportsRecruiting.Shared.Models;

namespace iSportsRecruiting.Shared.DTO
{
    public class LeadDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AthleteId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int Sport { get; set; }
        public int GraduationYear { get; set; }
        public DateTime AddedOn { get; set; } = DateTime.Now;
        public int Status { get; set; } = 1;
        
        public string SocialMedia { get; set; }
        public string Notes { get; set; }
        public string GPA { get; set; }

        public LeadModel ToModel()
        {
            return new LeadModel
            {
                Id = Id,
                User_Id = UserId,
                Athlete_Id = AthleteId,
                Full_Name = FullName,
                Email = Email,
                Phone = Phone,
                Sport = Sport,
                Graduation_Year = GraduationYear,
                Added_On = AddedOn,
                Status = Status,
                Social_Media = SocialMedia,
                Notes = Notes,
                GPA = GPA
            };
        }
    }
}
