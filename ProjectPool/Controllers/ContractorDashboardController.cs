using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectPool.Controllers
{
    [Authorize]
    public class ContractorDashboardController : Controller
    {
        private readonly DataContext _db;
        private readonly IConfiguration _configuration;

        public ContractorDashboardController(DataContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }


        [Route("Contractor/UpdateProfile")]
        public async Task<IActionResult> UpdateProfile(int id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            if (claimsIdentity.Claims.Count() == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            return View();
        }

    }
}
