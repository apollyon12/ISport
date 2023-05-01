using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSportsRecruiting.Shared.Models
{
    public class EntityBilling
    {
        public int Id { get; set; }
        public int Entity_Id { get; set; }
        public string Card_Number { get; set; } = "";
        public string Exp_Date { get; set; } = "";
        public string Card_Code { get; set; } = "";
        public string Zip { get; set; } = "";
        public string Plan { get; set; } = "";
        public string PromotionCode { get; set; } = "";
        public PromotionModel Promotion { get; set; } = new();
    }
}
