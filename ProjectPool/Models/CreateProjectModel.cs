using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class CreateProjectModel
    {

        [Required(ErrorMessage = "This field is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string Description { get; set; }
        [Required]
        public int DurationFrom { get; set; }
        [Required]
        public int DurationTo { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public int Cost { get; set; }
        [Required]
        public DateTime DatePosted { get; set; }

        public int EmployerID { get; set; }

        //for other table
        [Required]
        public int CategoryID { get; set; }
        [Required]
        public int SubCategoryID { get; set; }
        [Required]
        public string Skill { get; set; }
        [Required]
        public string Language { get; set; }

    }
}
