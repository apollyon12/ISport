using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class UserModel
    {
        public int User_Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int Games { get; set; }

        public UserDTO ToDTO()
        {
            return new UserDTO
            {
                Id = User_Id,
                Username = Username,
                Email = Email,
                Games = Games
            };
        }
    }
}
