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
        [Required]
        public string Duration { get; set; }

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
        //public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }

        [NotMapped]
        [Required]
        public string Skill { get; set; }

        [NotMapped]
        [Required]
        public string Language { get; set; }



    }
}
