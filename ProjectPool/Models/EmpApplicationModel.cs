using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class EmpApplicationModel
    {
        public string ContractorID { get; set; }
        public string EmployerID { get; set; }
        public string ProjectID { get; set; }
        public string Title { get; set; }
        public string FullName { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string ContractorSkills { get; set; }

        public string DayHourMin { get; set; }


    }
}
