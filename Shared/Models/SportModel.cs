using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class SportModel
    {
        public int Id { get; set; }
        public string Sport_Name { get; set; }
        public string Gender { get; set; }

        public SportDTO ToDTO()
        {
            return new SportDTO
            {
                Id = Id,
                Name = Sport_Name,
                Gender = Gender
            };
        }
    }
}
