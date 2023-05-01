using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class AthleteStatsModel
    {
        public int Id { get; set; }
        public int User_Id { get; set; }
        public int Sports_Id { get; set; }
        public int Position_Id { get; set; }
        public string Stats { get; set; }
        public int Year { get; set; }

        public AthleteStatsDTO ToDTO()
        {
            return new AthleteStatsDTO
            {
                Id = Id,
                UserId = User_Id,
                SportId = Sports_Id,
                PositionId = Position_Id,
                Stats = Stats,
                Year = Year
            };
        }
    }
}
