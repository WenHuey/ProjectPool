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
        public int ProjectID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Duration { get; set; }

        public string Status { get; set; }

        public int Cost { get; set; }

        public int EmployerID { get; set; }

        public string Category { get; set; }

        public string SubCategory { get; set; }

        public DateTime PostedAgo { get; set; }

        public int TotalBid { get; set; }

    }
}
