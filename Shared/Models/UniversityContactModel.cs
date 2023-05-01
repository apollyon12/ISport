namespace iSportsRecruiting.Shared.Models
{
    public class UniversityContactModel
    {
        public int Id { get; set; }
        public int University_Id { get; set; }
        public string Coach { get; set; }
        public string Email { get; set; }
        public string Sports { get; set; }
        public string AssisantCoach { get; set; }
        public string AssCoachEmail { get; set; }
    }
}
