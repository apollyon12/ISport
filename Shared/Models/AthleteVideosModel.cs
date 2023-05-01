using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class AthleteVideosModel
    {
        public int Id { get; set; }
        public int User_Id { get; set; }
        public string Video1 { get; set; }
        public string Title { get; set; }

        public AthleteVideosDTO ToDTO() 
        {
            return new AthleteVideosDTO
            {
                Id = Id,
                UserId = User_Id,
                Video = Video1,
                Title = Title
            };
        }
    }
}
