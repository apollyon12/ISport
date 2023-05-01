using System;
using System.Globalization;
using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class LeadModel
    {
        public int Id { get; set; }
        public int User_Id { get; set; }
        public int Athlete_Id { get; set; }
        public string Full_Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int Sport { get; set; }
        public int Graduation_Year { get; set; }
        public DateTime Added_On { get; set; }
        public int Status { get; set; }
        public string Social_Media { get; set; }
        public string Notes { get; set; }
        public string GPA { get; set; }

        public LeadDTO ToDTO()
        {
            return new LeadDTO
            {
                Id = Id,
                UserId = User_Id,
                AthleteId = Athlete_Id,
                FullName = Full_Name,
                Email = Email,
                Phone = Phone,
                Sport = Sport,
                GraduationYear = Graduation_Year,
                AddedOn = Added_On,
                Status = Status,
                SocialMedia = Social_Media,
                Notes = Notes,
                GPA = GPA
            };
        }
    }
}
