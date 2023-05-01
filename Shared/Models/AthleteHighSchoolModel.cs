namespace iSportsRecruiting.Shared.Models
{
    public class AthleteHighSchoolModel
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public string HighSchool { get; set; }
        public string Descriptions { get; set; }
        public int User_Id { get; set; }

        public AthleteHighSchoolDto ToDto()
        {
            return new AthleteHighSchoolDto
            {
                Id = Id,
                Year = Year,
                HighSchool = HighSchool,
                Descriptions = Descriptions,
                UserId = User_Id
            };
        }
    }

    public class AthleteHighSchoolDto
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public string HighSchool { get; set; }
        public string Descriptions { get; set; }
        public int UserId { get; set; }
    }
}
