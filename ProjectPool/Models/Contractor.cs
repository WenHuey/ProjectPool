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
        public int ContractorID { get; set; }
        public int UserID { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "State")]
        public string State { get; set; }
        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Required(ErrorMessage = "You must provide a phone number")]
        [Display(Name = "Contact Number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid contact number")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "Profile Description")]
        [StringLength(int.MaxValue)]
        public string ProfileDesc { get; set; }

        [Display(Name = "Reviews")]
        public string ReviewAverage { get; set; }

        public bool Deleted { get; set; }

        [NotMapped]
        [Required]
        [Display(Name = "Category")]
        public string CategoryID { get; set; }

        [NotMapped]
        [Required]
        [Display(Name = "Sub-Category")]
        public string SubCategory { get; set; }

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
