using Microsoft.AspNetCore.Mvc;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Controllers
{
    public class UserProfileController : Controller
    {
        [Route("EmployerProfile")]
        public IActionResult EmployerProfile()
        {
            return View();
        }

        [Route("ContractorProfile")]
        public IActionResult ContractorProfile()
        {
            return View();
        }

        //[Route("Signup")]
        //[HttpPost]
        //public IActionResult SignUp(SignUpModel signUpModel)
        //{
        //    if(ModelState.IsValid)
        //    {
        //        //write code

        //        ModelState.Clear();
        //    }
        //    return View();
        //}

        //[Route("Login")]
        //public IActionResult Login()
        //{
        //    return View();
        //}
    }
}
