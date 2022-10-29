using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class Interview
    {
        [Key]
        [Required]
        public int InterviewID { get; set; }
        

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "From Time")]
        public TimeSpan FromTime { get; set; }

        [Display(Name = "To Time")]
        public TimeSpan ToTime { get; set; }

        public int ApplicationID { get; set; }


        [Display(Name = "Meeting Link")]
        [StringLength(int.MaxValue)]
        public string Link { get; set; }

        public string Status { get; set; }


        



    }
}
