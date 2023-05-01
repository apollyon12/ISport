namespace iSportsRecruiting.Shared.DTO
{
    public class SportContactDTO
    {
        public int Id { get; set; }
        public int UniversityId { get; set; }
        public string Sports { get; set; }
        public string Coach { get; set; }
        public string Email { get; set; }
        public string AssisantCoach { get; set; }
        public string AssCoachEmail { get; set; }

        public SportContactDTO((int UniversityId, string Coach, string Email, string AssCoach, string AssEmail, string Sport, int? Id, Guid Guid) item)
        {
            Id = item.Id ?? 0;
            UniversityId = item.UniversityId;
            Sports = item.Sport;
            Coach = item.Coach;
            Email = item.Email;
            AssisantCoach = item.AssCoach;
            AssCoachEmail = item.AssEmail;
        }

        public SportContactDTO()
        {
            
        }
    }
}
