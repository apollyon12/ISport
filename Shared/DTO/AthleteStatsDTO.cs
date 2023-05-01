namespace iSportsRecruiting.Shared.DTO
{
    public class AthleteStatsDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SportId { get; set; }
        public int PositionId { get; set; }
        public string Stats { get; set; }
        public int Year { get; set; }
    }
}
