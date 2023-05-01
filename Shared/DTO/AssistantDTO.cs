using iSportsRecruiting.Shared.Models;

namespace iSportsRecruiting.Shared.DTO
{
    public class AssistantDTO
    {
        public int Id { get; set; }
        public int Type { get; set; } = 2;
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int UniversityId { get; set; }
        public string UniversityImage { get; set; }

        public AssistantModel ToModel()
        {
            return new AssistantModel
            {
                Id = Id,
                Type = Type,
                Full_Name = FullName,
                Email = Email,
                Phone = Phone,
                University_Id = UniversityId
            };
        }

        public Assistant ToStruct()
        {
            return new Assistant
            {
                Id = Id,
                Email = Email
            };
        }
    }

    public struct Assistant
    {
        public int Id { get; set; }
        public string Email { get; set; }
    }
}
