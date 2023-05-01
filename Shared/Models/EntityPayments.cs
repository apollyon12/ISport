using System;

namespace iSportsRecruiting.Shared.Models
{
    public class EntityPayments
    {
        public int Id { get; set; }
        public int Entity_Id { get; set; }
        public string Plan { get; set; }
        public string Amount { get; set; }
        public string Promotion_Code { get; set; }
        public DateTime Date { get; set; }
    }
}
