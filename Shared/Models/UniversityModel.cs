using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class UniversityModel
    {
        public int Id { get; set; }
        public string University { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public string Division { get; set; }
        public string Tuisionin { get; set; }
        public string Tuitionout { get; set; }
        public string GPA { get; set; }
        public string SAT { get; set; }
        public string ACT { get; set; }
        public string Cost_Of_Attendance { get; set; }
        public string Cost_Of_Attendance_Out { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public string AthleticWebSite { get; set; }
        public string Enrollement { get; set; }
        public string Image { get; set; }
        public bool IsFavorite { get; set; }
        public string Base64 { get; set; }

        public UniversityDTO ToDTO()
        {
            return new UniversityDTO
            {
                Id = Id,
                University = University,
                Address = Address,
                Website = Website,
                Division = Division,
                Tuisionin = Tuisionin,
                Tuitionout = Tuitionout,
                GPA = GPA,
                SAT = SAT,
                ACT = ACT,
                CostOfAttendance = Cost_Of_Attendance,
                CostOfAttendanceOut = Cost_Of_Attendance_Out,
                Type =Type,
                State = State,
                AthleticWebSite = AthleticWebSite,
                Enrollement = Enrollement,
                Image = Image,
                IsFavorite = IsFavorite
            };
        }
    }

    public static class UniversityDTOExtension
    {
        public static UniversityModel ToModel(this UniversityDTO university)
        {
            return new UniversityModel
            {
                Id = university.Id,
                University = university.University,
                Address = university.Address,
                Website = university.Website,
                Division = university.Division,
                Tuisionin = university.Tuisionin,
                Tuitionout = university.Tuitionout,
                GPA = university.GPA,
                SAT = university.SAT,
                ACT = university.ACT,
                Cost_Of_Attendance = university.CostOfAttendance,
                Cost_Of_Attendance_Out = university.CostOfAttendanceOut,
                Type = university.Type,
                State = university.State,
                AthleticWebSite = university.AthleticWebSite,
                Enrollement = university.Enrollement,
                Image = university.Image
            };
        }
    }
}
