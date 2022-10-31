using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly DataContext _db;
        private readonly IConfiguration _configuration;
        List<EmpActiveProjectModel> activeProject = new List<EmpActiveProjectModel>();
        //List<DashboardModel> projectCount = new List<DashboardModel>();
        public DashboardController(DataContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        
        public IActionResult Dashboard(DashboardModel model)
        {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            if (claimsIdentity.Claims.Count() == 0)
            {
                return RedirectToAction("Login", "Account");
            }

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
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

            var empID = _db.Employer.Where(x => x.UserID.ToString() == userID).Select(x => x.EmployerID).SingleOrDefault();

            //Connect db
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            getActive(empID);
            getRunning(empID);
            getApplication(empID);
            getInterview(empID);

            try
            {
                conn.Open();
                //Create command
                string query = "SELECT (SELECT COUNT(*) FROM Project WHERE[Status] = 'Active') AS ActiveCount, (SELECT COUNT(*) FROM Project WHERE[Status] = 'Running') AS RunningCount, (SELECT COUNT(*) FROM Interview WHERE[Status] = 'Pending') AS InterviewCount, (SELECT COUNT(*) FROM Application WHERE[Status] = 'Pending') AS ApplicationCount";
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

        public void getActive(int empID)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            
            conn.Open();
            //Create command
            string query = "SELECT TOP 1 Title, [Description], Cost, Duration FROM Project WHERE EmployerID='"+empID+"' AND [Status]='Active'";
            SqlCommand cmd = new SqlCommand(query, conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var activeProject = new DashboardModel()
                {
                    A_Title = dr["Title"].ToString(),
                    A_Desc = dr["Description"].ToString(),
                    A_Cost = dr["Cost"].ToString(),
                    A_Duration = dr["Duration"].ToString(),
                };
                ViewData["ActiveProject"] = activeProject;
            }

            conn.Close();

        }

        public void getRunning(int empID)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            conn.Open();
            //Create command
            string query = "SELECT TOP 1 Title, [Description], Cost, Duration FROM Project WHERE EmployerID='" + empID + "' AND [Status]='Running'";
            SqlCommand cmd = new SqlCommand(query, conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var runningProject = new DashboardModel()
                {
                    R_Title = dr["Title"].ToString(),
                    R_Desc = dr["Description"].ToString(),
                    R_Cost = dr["Cost"].ToString(),
                    R_Duration = dr["Duration"].ToString(),
                };
                ViewData["RunningProject"] = runningProject;
            }

            conn.Close();

        }

        public void getApplication(int empID)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            conn.Open();
            //Create command
            string query = "SELECT TOP 1 CONCAT(cat.[Name], ', ',p.SubCategoryName) as Tags, p.Title, CONCAT(c.LastName, ' ', c.FirstName) as FullName,(SELECT ABS(DATEDIFF(dd, CURRENT_TIMESTAMP, a.CreatedAt))) as Days, (SELECT ABS(DATEDIFF(hh, CURRENT_TIMESTAMP, a.CreatedAt)))as Hours, (SELECT ABS(DATEDIFF(mi, CURRENT_TIMESTAMP, a.CreatedAt))) as Minutes FROM [Application] a LEFT JOIN Project p on p.ProjectID = a.ProjectID LEFT JOIN Contractor c on c.ContractorID = a.ContractorID LEFT JOIN Category cat on cat.CategoryID = p.CategoryID WHERE a.EmployerID = '" + empID + "' AND a.[Status]= 'Pending'";
            SqlCommand cmd = new SqlCommand(query, conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var day = dr["Days"].ToString();
                var hr = dr["Hours"].ToString();
                var min = dr["Minutes"].ToString();
                string time = min + " minute(s) ago";

                if (Convert.ToInt32(hr) >= 24)
                {
                    time = day + " day(s) ago";
                }
                else if (Convert.ToInt32(min) >= 60)
                {
                    time = hr + " hour(s) ago";
                }

                var appProject = new DashboardModel()
                {
                    AP_Title = dr["Title"].ToString(),
                    AP_Tags = dr["Tags"].ToString(),
                    AP_Duration = time,
                    AP_FullName = dr["FullName"].ToString(),
                };
                ViewData["ApplicationProject"] = appProject;
            }

            conn.Close();

        }

        public void getInterview(int empID)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            conn.Open();
            //Create command
            string query = "SELECT TOP 1 iv.[Date],CONCAT(CONVERT(varchar(7),iv.FromTime,100),' - ',CONVERT(varchar(7),iv.ToTime,100)) as [IvTime], CONCAT(c.LastName, ' ', c.FirstName) as FullName, (SELECT DATEDIFF(dd, CURRENT_TIMESTAMP, iv.[Date])) as Days, (SELECT DATEDIFF(hh, CURRENT_TIMESTAMP, iv.[Date]))as Hours, (SELECT DATEDIFF(mi, CURRENT_TIMESTAMP, iv.[Date])) as Minutes FROM Interview iv LEFT JOIN[Application] a on a.ApplicationID = iv.ApplicationID LEFT JOIN Contractor c on c.ContractorID = a.ContractorID LEFT JOIN Project p on p.ProjectID = a.ProjectID WHERE a.EmployerID = '" + empID + "' AND iv.[Status]= 'Pending'";
            SqlCommand cmd = new SqlCommand(query, conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var Date = dr["Date"].ToString();
                var day = dr["Days"].ToString();
                var hr = dr["Hours"].ToString();
                var min = dr["Minutes"].ToString();
                bool datePassed = false;
                string time = min + " minute(s) more";

                if (Convert.ToInt32(day) <= 0 && Convert.ToInt32(hr) <= 0 && Convert.ToInt32(min) <= 0)
                {
                    datePassed = true;
                    time = "Interview Date Passed! Please make action!";
                }
                else
                {
                    if (Convert.ToInt32(hr) >= 24)
                    {
                        time = day + " day(s) more";
                    }
                    else if (Convert.ToInt32(min) >= 60)
                    {
                        time = hr + " hour(s) more";
                    }
                }

                var interview = new DashboardModel()
                {
                    I_Date = DateTime.Parse(Date).ToString("dd/MM/yyyy"),
                    I_FullName = dr["FullName"].ToString(),
                    I_IvTime = dr["IvTime"].ToString(),
                    I_Time = time,
                    datePassed = datePassed
                };
                ViewData["Interview"] = interview;
            }

            conn.Close();

        }


        [Route("Contractor")]
        [Route("Contractor/Dashboard")]
        public IActionResult ConDashboard()
        {
            return View();
        }

       
    }
}
