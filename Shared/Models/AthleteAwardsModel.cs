using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class AthleteAwardsModel
    {
        public int Id { get; set; }
        public string Awards { get; set; }
        public int Year { get; set; }

        public AthleteAwardsDTO ToDTO()
        {
            return new AthleteAwardsDTO
            {
                Id = Id,
                Awards = Awards,
                Year = Year
            };
        }
    }
}
