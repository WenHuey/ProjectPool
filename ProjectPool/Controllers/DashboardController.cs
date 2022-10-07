using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectPool.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IConfiguration _configuration;
        List<EmpActiveProjectModel> activeProject = new List<EmpActiveProjectModel>();
        //List<DashboardModel> projectCount = new List<DashboardModel>();
        public DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        
        public IActionResult Dashboard(DashboardModel model)
        {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            try
            {
                if(userID != null)
                {
                    if (userID == "2") //employer
                    {
                        model.UserId = Convert.ToInt32(userID);
                        return RedirectToAction("EmpDashboard", "Dashboard");
                    }
                    else if(userID == "3") //contractor
                    {
                        model.UserId = Convert.ToInt32(userID);
                        return RedirectToAction("ConDashboard", "Dashboard");
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return View();
        }

        [Route("Employer")]
        [Route("Employer/Dashboard")]
        public IActionResult EmpDashboard()
        {

            //Connect db
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            try
            {
                conn.Open();
                //Create command
                string query = "SELECT (SELECT COUNT(*) FROM Project WHERE[Status] = 'Active') AS ActiveCount, (SELECT COUNT(*) FROM Project WHERE[Status] = 'Running') AS RunningCount, (SELECT COUNT(*) FROM Interview) AS InterviewCount, (SELECT COUNT(*) FROM Application WHERE[Status] = 'Pending') AS ApplicationCount";
                SqlCommand cmd = new SqlCommand(query, conn);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var projectCount = new DashboardModel()
                    {
                        ActiveCount = Convert.ToInt32(dr["ActiveCount"]),
                        RunningCount = Convert.ToInt32(dr["RunningCount"]),
                        InterviewCount = Convert.ToInt32(dr["InterviewCount"]),
                        ApplicationCount = Convert.ToInt32(dr["ApplicationCount"]),
                    };
                    ViewData["ProjectCount"] = projectCount;
                }
                
                conn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            return View();
        }

        [Route("Contractor")]
        [Route("Contractor/Dashboard")]
        public IActionResult ConDashboard()
        {
            return View();
        }

       
    }
}
