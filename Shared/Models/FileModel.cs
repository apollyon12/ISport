namespace iSportsRecruiting.Shared.Models
{
    public class FileModel
    {
        public int Id { get; set; }
        public string File_Name { get; set; }
        public string Description { get; set; }
        public string Base64 { get; set; }
        public int RelateId { get; set; }
    }
}
