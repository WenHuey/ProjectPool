using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
    public class ContractorDashboardController : Controller
    {
        private readonly DataContext _db;
        private readonly IConfiguration _configuration;
        List<ConApplicationModel> applications = new List<ConApplicationModel>();
        List<ConAllModel> allProject = new List<ConAllModel>();
        List<ConInterviewModel> interviews = new List<ConInterviewModel>();
        List<ConRunningModel> running = new List<ConRunningModel>();

        public ContractorDashboardController(DataContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [Route("Contractor/AllProjects")]
        [HttpGet]
        public IActionResult ConAll()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "3")
            {
                return RedirectToAction("EmpApplication", "Dashboard");
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var conID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.ContractorID).SingleOrDefault();

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            if (allProject.Count > 0)
            {
                allProject.Clear();
            }
            try
            {
                conn.Open();
                //Create command
                SqlCommand cmd = new SqlCommand("Sp_ConDisplayAll", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ConID", conID);
                cmd.Parameters["@ConID"].Direction = ParameterDirection.Input;
                cmd.ExecuteNonQuery();

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var date = dr["AppliedOn"].ToString();
                    var company = dr["CompanyName"].ToString();
                    var fullName = dr["FullName"].ToString();
                    var name = company;

                    if (string.IsNullOrWhiteSpace(company))
                    {
                        name = fullName;
                    }


                    allProject.Add(new ConAllModel()
                    {
                        ProjectStatus = dr["ProjectStatus"].ToString(),
                        Application = dr["Application"].ToString(),
                        Title = dr["Title"].ToString(),
                        Name = name,
                        AppliedOn = DateTime.Parse(date).ToString("dd/MM/yyyy")
                    });
                }


                conn.Close();

            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Error! Retrieve Data Unsuccesful";
                throw ex;
            }

            return View(allProject);
        }

        #region Application
        [Route("Contractor/Application")]
        [HttpGet]
        public IActionResult ConApplication()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "3")
            {
                return RedirectToAction("EmpApplication", "Dashboard");
            }


            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

            //Connect db
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            if (applications.Count > 0)
            {
                applications.Clear();
            }
            try
            {
                conn.Open();
                //Create command
                SqlCommand cmd = new SqlCommand("Sp_ConDisplayApplication", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters["@UserID"].Direction = ParameterDirection.Input;
                cmd.ExecuteNonQuery();

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var date = dr["CreatedAt"].ToString();
                    var company = dr["CompanyName"].ToString();
                    var fullName = dr["FullName"].ToString();
                    var name = company;

                    if (string.IsNullOrWhiteSpace(company))
                    {
                        name = fullName;
                    }


                    applications.Add(new ConApplicationModel()
                    {
                        ProjectID = dr["ProjectID"].ToString(),
                        ApplicationStatus = dr["ApplicationStatus"].ToString(),
                        Title = dr["Title"].ToString(),
                        Name = name,
                        AppliedOn = DateTime.Parse(date).ToString("dd/MM/yyyy"),
                        TotalBid = dr["TotalBid"].ToString(),
                    });
                }


                conn.Close();

            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Error! Retrieve Data Unsuccesful";
                throw ex;
            }

            return View(applications);
        }

        #endregion

        #region Running
        [Route("Contractor/Running")]
        [HttpGet]
        public IActionResult ConRunning()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "3")
            {
                return RedirectToAction("EmpApplication", "Dashboard");
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var conID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.ContractorID).SingleOrDefault();

            try
            {
                SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                SqlDataReader dr;

                conn.Open();
                //Create command
                SqlCommand cmd = new SqlCommand("Sp_ConDisplayRunning", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ConID", conID);
                cmd.Parameters["@ConID"].Direction = ParameterDirection.Input;
                cmd.ExecuteNonQuery();

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var company = dr["CompanyName"].ToString();
                    var fullName = dr["FullName"].ToString();
                    var name = company;

                    if (string.IsNullOrWhiteSpace(company))
                    {
                        name = fullName;
                    }

                    running.Add(new ConRunningModel()
                    {
                        ApplicationID = dr["ApplicationID"].ToString(),
                        ProjectID = dr["ProjectID"].ToString(),
                        Title = dr["Title"].ToString(),
                        SubCategory = dr["SubCategory"].ToString(),
                        Cost = dr["Cost"].ToString(),
                        Name = name,
                        Progress = dr["Progress"].ToString()
                    });
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return View(running);
        }

        [HttpPost]
        public IActionResult UpdateProjectProgress(int id, int perc)
        {
            
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "3")
            {
                return RedirectToAction("EmpApplication", "Dashboard");
            }

            if (id == null || perc == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving ids";
                return RedirectToAction("ConRunning");
            }

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            try
            {
                conn.Open();
                //Create command
                string query = "UPDATE [Application] SET Progress = '" + perc + "' WHERE ApplicationID = '" + id + "'";

                SqlCommand cmd = new SqlCommand(query, conn);
                         
                cmd.ExecuteNonQuery();

                cmd.Connection.Close();
                TempData["SuccessMsg"] = "Project's Progress successfully updated!";

            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "An error occurred while updating progress";
                throw ex;
            }

            return RedirectToAction("ConRunning");
        }
        #endregion

        #region Interview

        [Route("Contractor/Interview")]
        [HttpGet]
        public IActionResult ConInterview()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "3")
            {
                return RedirectToAction("EmpApplication", "Dashboard");
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var conID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.ContractorID).SingleOrDefault();

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            if (interviews.Count > 0)
            {
                interviews.Clear();
            }
            try
            {
                conn.Open();
                //Create command
                SqlCommand cmd = new SqlCommand("Sp_ConDisplayInterview", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ConID", conID);
                cmd.Parameters["@ConID"].Direction = ParameterDirection.Input;
                cmd.ExecuteNonQuery();

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var date = dr["Date"].ToString();
                    var day = dr["Days"].ToString();
                    var hr = dr["Hours"].ToString();
                    var min = dr["Minutes"].ToString();
                    bool datePassed = false;
                    string time = min + " minute(s) more";
                    var company = dr["CompanyName"].ToString();
                    var fullName = dr["FullName"].ToString();
                    var name = company;

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

                    if (string.IsNullOrWhiteSpace(company))
                    {
                        name = fullName;
                    }


                    interviews.Add(new ConInterviewModel()
                    {
                        InterviewID = dr["InterviewID"].ToString(),
                        ProjectID = dr["ProjectID"].ToString(),
                        ApplicationID = dr["ApplicationID"].ToString(),
                        Title = dr["Title"].ToString(),
                        Name = name,
                        Date = DateTime.Parse(date).ToString("dd/MM/yyyy"),
                        Time = dr["Time"].ToString(),
                        Link = dr["Link"].ToString(),
                        TimeLeft = time,
                        DatePassed = datePassed,
                        Status = dr["Status"].ToString(),
                    });
                }


                conn.Close();

            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Error! Retrieve Data Unsuccesful";
                throw ex;
            }

            return View(interviews);
        }

        #endregion


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
