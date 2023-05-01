using System.Collections.Generic;
using System.Globalization;
using iSportsRecruiting.Shared.Models;

namespace iSportsRecruiting.Shared.DTO
{
    public class AthleteDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public int PositionId { get; set; }
        public string Position { get; set; }

        public int Position2Id { get; set; }
        public string Position2 { get; set; }

        public string ImageProfile { get; set; }
        public long NCAA { get; set; }
        public long NAIA { get; set; }
        public string CellPhone { get; set; }
        public string Birth { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string GPA { get; set; }
        public string ACT { get; set; }
        public string SAT { get; set; }
        public int Weight { get; set; }
        public decimal Height { get; set; }
        public string PersonalStatement { get; set; }
        public string CoachesEvaluation { get; set; }
        public string GraduationYear { get; set; }
        public decimal PercentCompletionProfile { get; set; }
        public string StatsStruct { get; set; }
        public string StatsStructValues { get; set; }
        public string GuardianName { get; set; }
        public string GuardianPhone { get; set; }
        public string GuardianEmail { get; set; }
        public string Plan { get; set; }

        public int SportId { get; set; }
        public string SportName { get; set; }
        public bool IsFavorite { get; set; }
        public string GpaFileName { get; set; }
        public string GpaFileOrigin { get; set; }
        public string ActFileName { get; set; }
        public string ActFileOrigin { get; set; }
        public string SatFileName { get; set; }
        public string SatFileOrigin { get; set; }
        public string HighSchool { get; set; }
        public bool ShowTut { get; set; }
        public bool Enabled { get; set; }
        
        public bool ResumeTut { get; set; }

        public IEnumerable<AthleteHonorsDTO> Honors { get; set; }
        public IEnumerable<AthleteAwardsDTO> Awards { get; set; }
        public IEnumerable<AhtleteCoachInfoDTO> Coaches { get; set; }
        public IEnumerable<AthleteVideosDTO> Videos { get; set; }
        public IEnumerable<AthleteStoriesDTO> Stories { get; set; }
        public IEnumerable<AthleteClubsDTO> Clubs { get; set; }
        public IEnumerable<AthleteHighSchoolDto> HighSchools { get; set; }

        public string GetHeight() => Height.ToString(CultureInfo.InvariantCulture).Replace(".", "'").Replace(",", "'") + "\"";
        public string GetWeight() => Weight + "lbs";
        public string GetEmail() => Email;
    }
}

