using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class Contractor
    {
        [Key]
        [Required]
        public int ContractorID { get; set; }
        public int UserID { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }
 
        [Display(Name = "State")]
        public string State { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }


        [Display(Name = "Contact Number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid contact number")]
        public string Phone { get; set; }


        [Display(Name = "Profile Description")]
        [StringLength(int.MaxValue)]
        public string ProfileDesc { get; set; }

        [Display(Name = "Reviews")]
        public double ReviewAverage { get; set; }

        [Required]
        public bool Deleted { get; set; }


        [Display(Name = "Category")]
        public int CategoryID { get; set; }


        [Display(Name = "Sub-Category")]
        public string SubCategoryName { get; set; }

        [NotMapped]
        [Required]
        [Display(Name = "Skill")]
        public string Skill { get; set; }

        [NotMapped]
        [Required]
        [Display(Name = "Language")]
        public string Language { get; set; }



    }
}
