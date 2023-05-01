using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class CoachModel
    {
        public int Id { get; set; }
        public int User_Id { get; set; }
        public string Username { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public int University_Id { get; set; }
        public int Sports_Id { get; set; }
        public int UniversityContact_Id { get; set; }
        public string University { get; set; }
        public string Email { get; set; }
        public string Image { get; set; }

        public CoachDTO ToDTO()
        {
            return new CoachDTO
            {
                Id = Id,
                UserId = User_Id,
                FirstName = First_Name,
                LastName = Last_Name,
                Username = Username,
                University = University,
                UniversityId = University_Id,
                UniversityContactId = UniversityContact_Id,
                Email = Email,
                SportId = Sports_Id
            };
        }
    }
}
