using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class ConApplicationModel
    {
        public string ProjectID { get; set; }
        public string ApplicationStatus { get; set; }
        
        public string Title { get; set; }
        public string ProjectStatus { get; set; }
        
        public string Name { get; set; }
        public string AppliedOn { get; set; }
        public string TotalBid { get; set; }



        public bool hasInterview { get; set; }


    }
}
