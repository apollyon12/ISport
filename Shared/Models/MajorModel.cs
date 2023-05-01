namespace iSportsRecruiting.Shared.Models
{
    public class MajorModel
    {
        public int Id { get; set; }
        public int Category_Id { get; set; }
        public string Name { get; set; }
    }

    public class MajorUniversityModel
    {
        public int University_Id { get; set; }
        public string Majors { get; set; }
        public string[] SplitedMajors { get; set; }
    }
}
