using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class ProjectListModel
    {
        [Key]
        public string ProjectID { get; set; }

        public string Title { get; set; }

        [StringLength(int.MaxValue)]
        public string Description { get; set; }

        [StringLength(int.MaxValue)]
        public string Scope { get; set; }

        public string Duration { get; set; }

        public string Status { get; set; }

        public string Cost { get; set; }

        //public int EmployerID { get; set; }

        //public string CategoryID { get; set; }

        public string SubCategoryName { get; set; }

        public string CategoryName { get; set; }

        public string PostedAgo { get; set; }

        public string TotalBid { get; set; }

        public string Skills { get; set; }




    }
}
