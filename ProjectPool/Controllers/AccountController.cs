using Microsoft.AspNetCore.Mvc;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Controllers
{
    public class AccountController : Controller
    {
        [Route("Signup")]
        public IActionResult SignUp()
        {
            return View();
        }

        [Route("Signup")]
        [HttpPost]
        public IActionResult SignUp(SignUpModel signUpModel)
        {
            if(ModelState.IsValid)
            {
                //write code

                ModelState.Clear();
            }
            return View();
        }

        [Route("/")]
        public IActionResult Login()
        {
            return View();
        }
    }
}
