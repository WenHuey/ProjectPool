using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class ApplicationController
    {
        [Key]
        public int ApplicationID { get; set; }
        public int ProjectID { get; set; }
        public int EmployerID { get; set; }

        public int ContractorID { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

       

    }
}
