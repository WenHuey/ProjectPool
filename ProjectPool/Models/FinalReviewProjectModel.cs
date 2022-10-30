using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class FinalReviewProjectModel
    {
        public string ContractorID { get; set; }
        public string ProjectID { get; set; }
        public string EmployerID { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }
        [Display(Name = "Scope")]
        public string Scope { get; set; }
        public string Tags { get; set; }
        public string Cost { get; set; }

        [Display(Name = "FullName")]
        public string FullName { get; set; }

        [Display(Name = "FinalAmount")]
        public string FinalAmount { get; set; }
        [Display(Name = "PaymentDesc")]
        public string PaymentDesc { get; set; }
        [Display(Name = "CompleteRate")]
        public string CompleteRate { get; set; }



    }
}
