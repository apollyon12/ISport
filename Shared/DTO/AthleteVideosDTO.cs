namespace iSportsRecruiting.Shared.DTO
{
    public class AthleteVideosDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Video { get; set; }
        public string Title { get; set; }
        public string Host => Video.Contains("you") ? "YouTube" : "Hudl";
    }
}
