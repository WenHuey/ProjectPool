using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class ReviewApplicationModel
    {
        //IDs
        public string ApplicationID { get; set; }       
        public string ProjectID { get; set; }
        public string EmployerID { get; set; }
        public string ContractorID { get; set; }

        //Project details
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string Cost { get; set; }
        public string Duration { get; set; }

        //contractor
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        [Display(Name = "Address")]
        public string Address { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Contact Number")]
        public string Phone { get; set; }
        [Display(Name = "ProfileDesc")]
        public string ProfileDesc { get; set; }
        [Display(Name = "Tags")]
        public string Tags { get; set; }
        [Display(Name = "Skill")]
        public string Skill { get; set; }
        [Display(Name = "Language")]
        public string Language { get; set; }

        //Application
        
        public string ConPitch { get; set; }

        //Interview

        [Display(Name = "Date")]
        public string IvDate { get; set; }

        [Display(Name = "From")]
        public string FromTime { get; set; }

        [Display(Name = "To")]
        public string ToTime { get; set; }

        [Display(Name = "Interview Link")]
        public string Link { get; set; }

        public bool hasPitch { get; set; }


    }
}
