using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class AthleteStoriesModel
    {
        public int Year { get; set; }
        public string HighSchool { get; set; }
        public string Descriptions { get; set; }

        public AthleteStoriesDTO ToDTO()
        {
            return new AthleteStoriesDTO
            {
                Year = Year,
                HighSchool = HighSchool,
                Description = Descriptions
            };
        }
    }
}
