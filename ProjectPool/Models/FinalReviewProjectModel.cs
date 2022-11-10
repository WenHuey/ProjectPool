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
        public string Duration { get; set; }

        [Display(Name = "FullName")]
        public string FullName { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Contact Number")]
        public string Phone { get; set; }

        [Display(Name = "Final Amount to Pay")]
        public string FinalAmount { get; set; }
        [Display(Name = "Payment Description")]
        public string PaymentDesc { get; set; }
        [Display(Name = "Project Completion Rate")]
        public string CompleteRate { get; set; }

        [Display(Name = "Review Description")]
        public string ReviewDesc { get; set; }
        [Display(Name = "Rating")]
        public string ReviewRate { get; set; }



    }
}
