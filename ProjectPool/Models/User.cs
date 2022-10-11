using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectPool.Models
{
    public class User
    {
        public int UserID { get ; set; }

        public int UserTypeID { get; set; }

        [Required(ErrorMessage = "Please enter your email address"), EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool Deleted { get; set; }

    }
}
