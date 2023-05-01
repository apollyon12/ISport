using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class AssistantModel
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string Full_Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int University_Id { get; set; }


        public AssistantDTO ToDTO()
        {
            return new AssistantDTO
            {
                Id = Id,
                Type = Type,
                FullName = Full_Name,
                Email = Email,
                Phone = Phone,
                UniversityId = University_Id
            };
        }
    }
}
