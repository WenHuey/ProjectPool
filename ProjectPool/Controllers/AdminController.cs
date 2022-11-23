using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectPool.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        [AllowAnonymous]
        [Route("Admin")]
        [HttpGet]
        public IActionResult Index()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (usertype != null)
                {
                    if (usertype == "2") //employer
                    {
                        return RedirectToAction("EmpDashboard", "Dashboard");
                    }
                    else if (usertype == "3") //contractor
                    {
                        return RedirectToAction("ConDashboard", "Dashboard");
                    }
                    else if (usertype == "1")
                    {
                        return RedirectToAction("AdminDashboard", "Admin");
                    }
                }
                //return View("~/Views/Dashboard/Dashboard.cshtml");
                //return RedirectToAction("Welcome");
            }
            return View();
        }

        [Route("Admin")]
        [HttpPost]
        public IActionResult Index(AdminLoginModel model)
        {
            return View();
        }

        [Route("Admin/Dashboard")]
        [HttpGet]
        public IActionResult AdminDashboard(AdminLoginModel model)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if(usertype != "1")
            {

            }
            return View();
        }
    }
}
