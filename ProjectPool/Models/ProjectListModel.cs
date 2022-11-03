using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class ContractorListModel
    {
        
        public string ContractorID { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string ProfileDesc { get; set; }

        public string ReviewAverage { get; set; }

        public string SubCategoryName { get; set; }

        public string CategoryName { get; set; }

        public bool HasReview { get; set; }

        public bool HasSub { get; set; }




    }
}
