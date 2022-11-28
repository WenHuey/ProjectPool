using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class AdminDashboardModel
    {
     
        public string ProjectCount { get; set; }

        public string UserCount { get; set; }

        public string EmployerCount { get; set; }

        public string ContractorCount { get; set; }

        public string UserID { get; set; }

        public string EmployerID { get; set; }
        public string Emp_FN { get; set; }
        public string Emp_LN { get; set; }
        public string Emp_CN { get; set; }

        public string ContractorID { get; set; }
        public string Con_FN { get; set; }
        public string Con_LN { get; set; }
        public string Con_Location { get; set; }


    }
}
