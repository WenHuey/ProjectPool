using Microsoft.AspNetCore.Mvc;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Controllers
{
    public class DashboardController : Controller
    {
        [Route("Dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }

    
    }
}
