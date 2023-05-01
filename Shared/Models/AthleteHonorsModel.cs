using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class AthleteHonorsModel
    {
        public int Id { get; set; }
        public string Honor_Roll { get; set; }
        public string Year { get; set; }

        public AthleteHonorsDTO ToDTO()
        {
            return new AthleteHonorsDTO
            {
                Id = Id,
                HonorRoll = Honor_Roll,
                Year = int.TryParse(Year, out int parsedYear) ? parsedYear : 0
            };
        }
    }
}
