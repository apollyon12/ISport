using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class AthleteClubsModel
    {
        public int Id { get; set; }
        public string Club_Name { get; set; }
        public int Competition_Year { get; set; }

        public AthleteClubsDTO ToDTO()
        {
            return new AthleteClubsDTO
            {
                Id = Id,
                ClubName = Club_Name,
                CompetitionYear = Competition_Year
            };
        }
    }
}
