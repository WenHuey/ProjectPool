using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Controllers
{
    public class EmployerDashboardController : Controller
    {
        private readonly IConfiguration _configuration;
        List<EmpActiveProjectModel> activeProject = new List<EmpActiveProjectModel>();
        //List<DashboardModel> projectCount = new List<DashboardModel>();
        public EmployerDashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //Active Project
        [Route("Employer/Active")]
        public IActionResult EmpActive()
        {

            //Connect db
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            if (activeProject.Count > 0)
            {
                activeProject.Clear();
            }
            try
            {
                conn.Open();
                //Create command
                string query = "SELECT  TOP(10) COUNT(*) TotalCount, p.ProjectID, p.Title, c.[Name] as Category, p.Cost FROM Project p Left JOIN[Application] a ON a.ProjectID = p.ProjectID Left JOIN[Category] c ON c.CategoryID = p.CategoryID WHERE p.Status = 'Active' GROUP BY p.ProjectID, p.Title, c.[Name], p.Cost";
                SqlCommand cmd = new SqlCommand(query, conn);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    activeProject.Add(new EmpActiveProjectModel()
                    {
                        TotalCount = dr["TotalCount"].ToString(),
                        Title = dr["Title"].ToString(),
                        Category = dr["Category"].ToString(),
                        Cost = dr["Cost"].ToString()
                    });
                }
                conn.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return View(activeProject);
        }

        [Route("EmpRunningProject")]
        public IActionResult EmpRunningProject()
        {
            return View();
        }

        [Route("Project")]
        public IActionResult ConPendingProject()
        {
            return View();
        }


    }
}
