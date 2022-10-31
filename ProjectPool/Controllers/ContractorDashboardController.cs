using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        
        public async Task<IActionResult> ViewProfile()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            if (claimsIdentity.Claims.Count() == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            if (userID == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data";
                return RedirectToAction("ConDashboard", "Dashboard");
            }

            var ConID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x=>x.ContractorID).SingleOrDefault();
            var getDetails = await _db.Contractor.FindAsync(ConID);

            if (getDetails == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data";
                return RedirectToAction("ConDashboard", "Dashboard");
            }

            return View(getDetails);
        }

        [Route("Contractor/UpdateProfile")]
        public IActionResult UpdateProfile(int id)
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
