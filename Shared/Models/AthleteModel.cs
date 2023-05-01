using iSportsRecruiting.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iSportsRecruiting.Shared.Services;

namespace iSportsRecruiting.Shared.Models
{
    public class AthleteModel
    {
        public int Id { get; set; }
        public int User_Id { get; set; }
        public string First_Name { get; set; }
        public string Middle_Name { get; set; }
        public string Last_Name { get; set; }
        public string NCAA { get; set; }
        public string NAIA { get; set; }
        public string Cel_Phone { get; set; }
        public string Position_Id { get; set; }
        public int Position2_Id { get; set; }
        public string Image_Profile { get; set; }
        public string Birth { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string GPA { get; set; }
        public string ACT { get; set; }
        public string SAT { get; set; }
        public string Weight { get; set; }
        public string Height { get; set; }
        public string Personal_Statement { get; set; }
        public string Coaches_Evaluation { get; set; }

        public string Hs_Grad_Year { get; set; }
        public string G_First_Name { get; set; }
        public string G_Phone { get; set; }
        public string G_Mail { get; set; }
        public string High_School { get; set; }
        public string Sport_Name { get; set; }
        public bool Show_Tut { get; set; }
        public bool Enabled { get; set; }

        public async Task<AthleteDTO> ToDtoAsync(IDatabaseContext context) 
        {
            return new AthleteDTO
            {
                Id = Id,
                UserId = User_Id,
                FirstName = First_Name,
                MiddleName = Middle_Name,
                LastName = Last_Name,
                NCAA = NCAA is not null ? (long.TryParse(NCAA, out long ncaa) ? ncaa : 0) : 0,
                NAIA = NAIA is not null ? (long.TryParse(NAIA, out long naia) ? naia : 0) : 0,
                CellPhone = Cel_Phone,
                Email = Email,
                Address = Address,
                Address2 = Address2,
                Country = Country,
                City = City,
                ImageProfile = Image_Profile,
                Birth = Birth,
                State = State,
                GPA = GPA,
                ACT = ACT,
                SAT = SAT,
                Weight = Weight is not null ? (int.TryParse(Weight, out int weight) ? weight : 0) : 0,
                Height = Height is not null ? (decimal.TryParse(Height.Replace("’", "."), out decimal height) ? height : 0) : 0,
                Position =  Position_Id is not null ? (await context.GetAthletePositionAsync(int.Parse(Position_Id)))?.Positions : string.Empty,
                Position2 = Position2_Id != 0 ? (await context.GetAthletePositionAsync(Position2_Id))?.Positions : string.Empty,
                PersonalStatement = Personal_Statement,
                CoachesEvaluation = Coaches_Evaluation,
                GraduationYear = Hs_Grad_Year,
                GuardianName = G_First_Name,
                GuardianEmail = G_Mail,
                GuardianPhone = G_Phone,
                HighSchool = High_School,
                ShowTut = Show_Tut,
                Enabled = Enabled
            };
        }

        public AthleteDTO ToSimpleDTO()
        {
            return new AthleteDTO
            {
                Id = Id,
                UserId = User_Id,
                FirstName = First_Name,
                MiddleName = Middle_Name,
                LastName = Last_Name,
                Email = Email,
                GPA = GPA,
                ImageProfile = Image_Profile,
                GraduationYear = Hs_Grad_Year,
                HighSchool = High_School,
                PersonalStatement = Personal_Statement,
                SportName = Sport_Name,
                ShowTut = Show_Tut,
                Enabled = Enabled
            };
        }
    }
}
