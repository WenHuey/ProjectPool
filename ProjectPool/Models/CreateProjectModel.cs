using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class CreateProjectModel
    {
        [Key]
        public int ProjectID { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(int.MaxValue)]
        public string Description { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [StringLength(int.MaxValue)]
        public string Scope { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public string Duration { get; set; }

        
        public string Status { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string Cost { get; set; }
        [Required]
        public DateTime DatePosted { get; set; }

        public int EmployerID { get; set; }

        public bool Deleted { get; set; }

        //for other table
        [Required(ErrorMessage = "This field is required")]
        public int CategoryID { get; set; }
        //public string CategoryName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string SubCategoryName { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "This field is required")]
        public string Skill { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "This field is required")]
        public string Language { get; set; }



    }
}
