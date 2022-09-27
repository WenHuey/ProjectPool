using Microsoft.AspNetCore.Mvc;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Controllers
{
    public class ProfileController : Controller
    {
        [Route("CreateProfile")]
        public IActionResult CreateProfile()
        {
            return View();
        }

        //[HttpPost]
        //[Route("CreateProfile")]
        //public IActionResult CreateProfile()
        //{
        //    return View();
        //}


    }
}
