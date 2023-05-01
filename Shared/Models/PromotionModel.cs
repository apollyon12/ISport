using System;

namespace iSportsRecruiting.Shared.Models
{
    public class PromotionModel
    {
        public int Id { get; set; }
        public string Code { get; set;}
        public int Discount { get; set; } = 1;
        public string Description { get; set; }
        public DateTime Expiration { get; set; }
        public string Plan_Id { get; set; }
    }
}
