using iSportsRecruiting.Shared.DTO;

namespace iSportsRecruiting.Shared.Models
{
    public class SportContactModel
    {
        public int Id { get; set; }
        public int University_Id { get; set; }
        public string Sports { get; set; }
        public string Coach { get; set; }
        public string Email { get; set; }
        public string AssisantCoach { get; set; }
        public string AssCoachEmail { get; set; }

        public SportContactDTO ToDTO()
        {
            return new SportContactDTO
            {
                Id = Id,
                UniversityId = University_Id,
                Sports = Sports,
                Coach = Coach,
                Email = Email,
                AssisantCoach = AssisantCoach,
                AssCoachEmail = AssCoachEmail
            };
        }
    }

    public static class SportContactDTOExtension
    {
        public static SportContactModel ToModel(this SportContactDTO sportContact)
        {
            return new SportContactModel
            {
                Id = sportContact.Id,
                University_Id = sportContact.UniversityId,
                Sports = sportContact.Sports,
                Coach = sportContact.Coach,
                Email = sportContact.Email,
                AssisantCoach = sportContact.AssisantCoach,
                AssCoachEmail = sportContact.AssCoachEmail

            };
        }
    }
}
