using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class ProjectDetailsModel
    {
        public string UserID { get; set; }
        public string EmployerID { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string TotalBid { get; set; }

        public string ProjectID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Scope { get; set; }

        public string Duration { get; set; }

        public string Cost { get; set; }

        public string DatePosted { get; set; }

        public string SubCategoryName { get; set; }

        public string CategoryName { get; set; }
        public string Skills { get; set; }
        public string Language { get; set; }


        //public string PostedAgo { get; set; }
        public string Pitch { get; set; }

        public bool isApplied { get; set; }

        public bool isEmp { get; set; }


    }
}
