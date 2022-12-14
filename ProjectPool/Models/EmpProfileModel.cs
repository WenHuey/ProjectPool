using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class EmpProfileModel
    {
        public int EmployerID { get; set; }


        public string FullName { get; set; }
        
        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        [Display(Name = "First Name")]
        public string FName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "State")]
        public string State { get; set; }
        [Required]
        [Display(Name = "Contact Number")]
        public string Phone { get; set; }
        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        [Required]
        [Display(Name = "Profile Description")]
        [StringLength(int.MaxValue)]
        public string ProfileDesc { get; set; }
        [Required]
        [Display(Name = "State")]
        public string Country { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        public string CategoryID { get; set; }

        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Cost { get; set; }
        public string Duration { get; set; }

        public string DayHourMin { get; set; }

        public bool isCon { get; set; }

        public string[] Skills { get; set; }
    }
}
