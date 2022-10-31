using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class EmpClosedModel
    {
     
        public string ProjectID { get; set; }

        public int EmployerID { get; set; }


        public string Title { get; set; }

        public string Cost { get; set; }

        public string CompletionRate { get; set; }

        public string Amount { get; set; }

        public string PaymentDate { get; set; }

        public string FullName { get; set; }

        public string Status { get; set; }




    }
}
