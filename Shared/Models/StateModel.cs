using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class StateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Country_Id { get; set; }

        public StateDTO ToDTO()
        {
            return new StateDTO
            {
                Id = Id,
                Name = Name,
                CountryId = Country_Id
            };
        }
    }
}
