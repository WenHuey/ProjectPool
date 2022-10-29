using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class EmpInterviewModel
    {
        //Ids
        public string InterviewID { get; set; }
        public string ApplicationID { get; set; }
        public string ContractorID { get; set; }
        public string ProjectID { get; set; }


        public string FullName { get; set; }
        public string Title { get; set; }

        public string Date { get; set; }
        public string Time { get; set; }
        public string Link { get; set; }

        public string TimeLeft { get; set; }

        public bool DatePassed { get; set; }

    }
}
