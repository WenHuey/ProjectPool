using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class EmpProfileModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string State { get; set; }
        public string Phone { get; set; }
        public string CompanyName { get; set; }

        [StringLength(int.MaxValue)]
        public string ProfileDesc { get; set; }

        public string Country { get; set; }

        public string Email { get; set; }

    }
}
