using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class DivisionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public DivisionDTO ToDTO()
        {
            return new DivisionDTO
            {
                Id = Id,
                Name = Name
            };
        }
    }
}
