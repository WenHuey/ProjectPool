using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class Project
    {
        [Key]
        public int ProjectID { get; set; }
        public string Title { get; set; }

        public string Description { get; set; }
        public string Duration { get; set; }
        public string Status { get; set; }
        public int Cost { get; set; }
        public int EmployerID { get; set; }
        public DateTime DatePosted { get; set; }
        public int CategoryID { get; set; }
        public string SubCategoryName { get; set; }



    }
}
