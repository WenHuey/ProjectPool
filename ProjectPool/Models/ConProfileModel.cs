using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class ConProfileModel
    {
        public int ContractorID { get; set; }

        public string FullName { get; set; }

        [Display(Name = "First Name")]
        public string FName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        [Display(Name = "Contact Number")]
        public string Phone { get; set; }

        [Display(Name = "Profile Description")]
        [StringLength(int.MaxValue)]
        public string ProfileDesc { get; set; }

        [Display(Name = "State")]
        public string Country { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        public string ReviewAverage { get; set; }
        public string CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }

        public string EditSkill { get; set; }
        public string EditLang { get; set; }


        public string[] Skills { get; set; }

        public bool isCon { get; set; }

        [StringLength(int.MaxValue)]
        public string PortfolioString { get; set; }


        public string P_Title { get; set; }
        public string P_Desc { get; set; }
        public string P_SubCategory { get; set; }
        public string P_Category { get; set; }
        public string[] P_Skills { get; set; }

        public string PortfolioID { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public IFormFile Portfolio { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string PF_Title { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        [StringLength(int.MaxValue)]
        public string PF_Desc { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string PF_Skills { get; set; }

        public string[] PF_SkillArray { get; set; }


    }
}
