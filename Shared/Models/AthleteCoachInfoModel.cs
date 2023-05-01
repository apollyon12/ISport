using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class AthleteCoachInfoModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Club_Name { get; set; }
        public string Type { get; set; }

        public AhtleteCoachInfoDTO ToDTO()
        {
            return new AhtleteCoachInfoDTO
            {
                Id = Id,
                Name = Name,
                Phone = Phone,
                Email = Email,
                ClubName = Club_Name,
                Type = Type
            };
        }
    }
}
