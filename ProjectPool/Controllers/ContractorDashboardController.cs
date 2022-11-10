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
        ConProfileModel conProfile = new ConProfileModel();
        List<ConProfileModel> doneProject = new List<ConProfileModel>();

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


        [Route("Contractor/Details/{id}/{uID}")]
        [HttpGet]
        public IActionResult ConDetails(int id, int uID)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var conID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.ContractorID).SingleOrDefault();
            var isCon = true;
            if(uID != Convert.ToInt32(userID))
            {
                var empID = _db.Employer.Where(x => x.UserID.ToString() == userID).Select(x => x.EmployerID).SingleOrDefault();
                return RedirectToAction("EmpDetails", "EmployerDashboard", new { id = empID, uID = 0});
            }

            if (usertype != "3")
            {
                isCon = false;
            }
            else if (id != conID)
            {
                isCon = false;
            }

            if (uID.Equals(Convert.ToInt32(userID)))
            {
                id = conID;
                isCon = true;
            }

            getDoneProjects(id);

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            conn.Open();

            try
            {
                string query = "SELECT c.*, u.Email, CONCAT(c.[State], ', ', c.Country) as [Address], sl.Skills, cat.[Name] as CategoryName FROM Contractor c LEFT JOIN [User] u on u.UserID = c.UserID LEFT JOIN Category cat on cat.CategoryID = c.CategoryID LEFT JOIN SkillsList sl on sl.ContractorID = c.ContractorID WHERE c.ContractorID = '" + id+"' AND c.Deleted = 0";
                SqlCommand cmd = new SqlCommand(query, conn);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var fname = dr["FirstName"].ToString();
                    var lname = dr["LastName"].ToString();
                    var full = lname + " " + fname;
                    string skills = dr["Skills"].ToString();
                    string[] array = skills.Split(',');

                    conProfile.ContractorID = Convert.ToInt32(dr["ContractorID"]);
                    conProfile.FullName = full;
                    conProfile.Email = dr["Email"].ToString();
                    conProfile.Address = dr["Address"].ToString();
                    conProfile.Phone = dr["Phone"].ToString();
                    conProfile.ProfileDesc = dr["ProfileDesc"].ToString();
                    conProfile.ReviewAverage = dr["ReviewAverage"].ToString();
                    conProfile.CategoryName = dr["CategoryName"].ToString();
                    conProfile.SubCategoryName = dr["SubCategoryName"].ToString();
                    conProfile.isCon = isCon;
                    conProfile.Skills = array;

                }

                conn.Close();

                
                //conn.Open();

                //string query2 = "SELECT p.Title, p.[Description], p.Cost, p.Duration, (SELECT ABS(DATEDIFF(dd, CURRENT_TIMESTAMP, DatePosted))) as Days, (SELECT ABS(DATEDIFF(hh, CURRENT_TIMESTAMP, DatePosted)))as Hours, (SELECT ABS(DATEDIFF(mi, CURRENT_TIMESTAMP, DatePosted))) as Minutes FROM Project p WHERE p.EmployerID = '" + id + "' AND p.[Status] = 'Active'";
                //cmd = new SqlCommand(query2, conn);

                //cmd.ExecuteNonQuery();
                //dr = cmd.ExecuteReader();

                //while (dr.Read())
                //{
                //    var day = dr["Days"].ToString();
                //    var hr = dr["Hours"].ToString();
                //    var min = dr["Minutes"].ToString();
                //    string time = min + " minute(s) ago";

                //    if (Convert.ToInt32(hr) >= 24)
                //    {
                //        time = day + " day(s) ago";
                //    }
                //    else if (Convert.ToInt32(min) >= 60)
                //    {
                //        time = hr + " hour(s) ago";
                //    }

                //    active.Add(new EmpProfileModel()
                //    {
                //        Title = dr["Title"].ToString(),
                //        Description = dr["Description"].ToString(),
                //        Cost = dr["Cost"].ToString(),
                //        Duration = dr["Duration"].ToString(),
                //        DayHourMin = time,
                //    });

                //    ViewData["ActiveProject"] = active;


                //}

                //conn.Close();
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Unsuccesful";
            }

            return View(conProfile);
        }

        private bool hasDoneProject(int? conID)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            conn.Open();
            //Create command
            string query = "SELECT COUNT(*) as [Count] FROM Payment pm  LEFT JOIN Contractor c on c.ContractorID = pm.ContractorID WHERE pm.ContractorID = '"+conID+"' AND c.Deleted=0";
            SqlCommand cmd = new SqlCommand(query, conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var count = dr["Count"].ToString();
                if(Convert.ToInt32(count) > 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public void getDoneProjects(int conID)
        {
            if (hasDoneProject(conID))
            {
                SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                SqlDataReader dr;

                conn.Open();
                //Create command
                string query = "SELECT p.Title, p.[Description],p.SubCategoryName, cat.[Name] as CategoryName, sl.Skills FROM Payment pm LEFT JOIN Project p on pm.ProjectID = p.ProjectID LEFT JOIN Category cat on cat.CategoryID = p.CategoryID LEFT JOIN SkillsList sl on sl.ProjectID = p.ProjectID WHERE pm.ContractorID = '"+conID+"'";
                SqlCommand cmd = new SqlCommand(query, conn);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    string skills = dr["Skills"].ToString();
                    string[] array = skills.Split(',');

                    doneProject.Add(new ConProfileModel()                     
                    {
                        P_Title = dr["Title"].ToString(),
                        P_Desc = dr["Description"].ToString(),
                        P_Category = dr["CategoryName"].ToString(),
                        P_SubCategory = dr["SubCategoryName"].ToString(),
                        P_Skills = array,
                    });
                    ViewData["DoneProject"] = doneProject;
                }

                conn.Close();
            }
            else
            {
                ViewData["DoneProject"] = "0";
            }
            

        }



        [Route("Contractor/EditDetails/{id}")]
        [HttpGet]
        public IActionResult EditConProfile(int id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "3")
            {
                return RedirectToAction("EmpDashboard", "Dashboard");
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var conID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.ContractorID).SingleOrDefault();

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            conn.Open();

            try
            {
                string query = "SELECT c.*, u.Email FROM Contractor c LEFT JOIN [User] u on u.UserID = c.UserID WHERE c.ContractorID = '"+conID+"'";
                SqlCommand cmd = new SqlCommand(query, conn);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var phone = dr["Phone"].ToString();
                    var state = dr["State"].ToString();
                    var country = dr["Country"].ToString();
                    var desc = dr["ProfileDesc"].ToString();

                    if (string.IsNullOrWhiteSpace(phone))
                    {
                        phone = " ";
                    }

                    if (string.IsNullOrWhiteSpace(desc))
                    {
                        desc = " ";
                    }


                    conProfile.ContractorID = Convert.ToInt32(dr["ContractorID"]);
                    conProfile.FirstName = dr["FirstName"].ToString();
                    conProfile.LastName = dr["LastName"].ToString();
                    conProfile.Email = dr["Email"].ToString();
                    conProfile.Phone = dr["Phone"].ToString();
                    conProfile.ProfileDesc = dr["ProfileDesc"].ToString();
                    conProfile.State = dr["State"].ToString();
                    conProfile.Country = dr["Country"].ToString();

                }
            }
            catch (Exception ex)
            {
                TempData["message"] = "Unsuccesful";
            }
            conn.Close();

            return View(conProfile);
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
