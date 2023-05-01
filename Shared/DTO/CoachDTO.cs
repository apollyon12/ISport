namespace iSportsRecruiting.Shared.DTO
{
    public class CoachDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public int UniversityId { get; set; }
        public int UniversityContactId { get; set; }
        public string University { get; set; }
        public string Email { get; set; }
        public string Cipher { get; set; }
        public int SportId { get; set; }
        public string UniversityImage { get; set; }
        public bool IsCoach { get; set; }
    }
}
