using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class SkillsList
    {
        [Key]
        [Required]
        public int SkillsListID { get; set; }

        public int? ProjectID { get; set; }
        public int? ContractorID { get; set; }

        public int? PortfolioID { get; set; }

        public string Skills { get; set; }
    }
}
