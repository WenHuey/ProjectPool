using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class PaymentModel
    {
        public string ContractorID { get; set; }

        public string EmployerID { get; set; }

        public string ProjectID { get; set; }

        public string PaymentDate { get; set; }

        public string Description { get; set; }

        public string Amount { get; set; }

        public string CompletionRate { get; set; }



    }
}
