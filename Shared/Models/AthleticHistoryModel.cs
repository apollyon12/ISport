using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSportsRecruiting.Shared.Models
{
    public class AthleticHistoryModel
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public string HighSchool { get; set; }
        public string Descriptions { get; set; }
        public int User_Id { get; set; }
    }
}
