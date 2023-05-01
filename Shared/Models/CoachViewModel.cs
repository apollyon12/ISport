using System;

namespace iSportsRecruiting.Shared.Models
{
    public class CoachViewModel
    {
        public int Id { get; set; }
        public int Coach_Id { get; set; }
        public int Athlete_Id { get; set; }
        public string University_Name { get; set; }
        public string Coach_Name { get; set; }
        public string Full_Name { get; set; }
        public int University_Id { get; set; }
        public string University_Image { get; set; }
        public int Count { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
