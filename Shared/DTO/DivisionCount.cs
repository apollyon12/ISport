namespace iSportsRecruiting.Shared.DTO
{
    public class DivisionCount
    {
        public string Division { get; set; }
        public int Count { get; set; }

        public DivisionCount(string division, int count)
        {
            Division = division;
            Count = count;
        }
    }
}