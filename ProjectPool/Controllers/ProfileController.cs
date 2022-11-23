using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectPool.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly DataContext _db;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        EmpProfileModel profile = new EmpProfileModel();
        List<EmpProfileModel> active = new List<EmpProfileModel>();
        List<ConProfileModel> doneProject = new List<ConProfileModel>();
        ConProfileModel conProfile = new ConProfileModel();
        List<ConProfileModel> portfolio = new List<ConProfileModel>();

        public ProfileController(DataContext db, IConfiguration configuration, 
            IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        #region Employer
        [Route("Employer/EditDetails/{id}")]
        [HttpGet]
        public IActionResult EditEmpProfile(int id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                return RedirectToAction("ConProfile", "ContractorDashboard");
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var empID = _db.Employer.Where(x => x.UserID.ToString() == userID).Select(x => x.EmployerID).SingleOrDefault();

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            conn.Open();

            try
            {
                string query = "SELECT e.*, u.Email, CONCAT(e.[State], ', ', e.Country) as Address FROM Employer e LEFT JOIN [User] u on u.UserID = e.UserID WHERE e.EmployerID = '" + empID + "'";
                SqlCommand cmd = new SqlCommand(query, conn);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var phone = dr["Phone"].ToString();
                    var state = dr["State"].ToString();
                    var country = dr["Country"].ToString();
                    var address = dr["Address"].ToString();
                    var desc = dr["ProfileDesc"].ToString();
                    var cName = dr["CompanyName"].ToString();

                    if (string.IsNullOrWhiteSpace(phone))
                    {
                        phone = " ";
                    }

                    if (string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(country))
                    {
                        address = " ";
                    }

                    if (string.IsNullOrWhiteSpace(desc))
                    {
                        desc = " ";
                    }

                    if (string.IsNullOrWhiteSpace(cName))
                    {
                        cName = " ";
                    }

                    profile.EmployerID = Convert.ToInt32(dr["EmployerID"]);
                    profile.FName = dr["FirstName"].ToString();
                    profile.LastName = dr["LastName"].ToString();
                    profile.Email = dr["Email"].ToString();
                    profile.Address = address;
                    profile.Phone = phone;
                    profile.ProfileDesc = desc;
                    profile.CompanyName = cName;
                    profile.State = state;
                    profile.Country = country;

                }
            }
            catch (Exception ex)
            {
                TempData["message"] = "Unsuccesful";
            }
            conn.Close();

            return View(profile);
        }

        [Route("Employer/EditDetails/{id}")]
        [HttpPost]
        public IActionResult EditEmpProfile(int id, EmpProfileModel model)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var empID = _db.Employer.Where(x => x.UserID.ToString() == userID).Select(x => x.EmployerID).SingleOrDefault();

            if (id != empID)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieve ID";
                return RedirectToAction("EmpDetails", new { id = id, uID = userID });
            }

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("Sp_UpdateProfile", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {

                cmd.Parameters.AddWithValue("@EmpID", empID);
                cmd.Parameters["@EmpID"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters["@UserID"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Fname", model.FName);
                cmd.Parameters["@Fname"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Lname", model.LastName);
                cmd.Parameters["@Lname"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@State", model.State);
                cmd.Parameters["@State"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Country", "Malaysia");
                cmd.Parameters["@Country"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Phone", model.Phone);
                cmd.Parameters["@Phone"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@CompanyName", model.CompanyName);
                cmd.Parameters["@CompanyName"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Pdesc", model.ProfileDesc);
                cmd.Parameters["@Pdesc"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters["@Email"].Direction = ParameterDirection.Input;


                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Fail to update data!";
                throw ex;
            }
            cmd.Connection.Close();

            return RedirectToAction("EmpDetails", new { id = id, uID = userID });
        }

        //display profile
        [Route("Employer/Details/{id}/{uID}")]
        [HttpGet]
        public IActionResult EmpDetails(int id, int uID)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var empID = _db.Employer.Where(x => x.UserID.ToString() == userID).Select(x => x.EmployerID).SingleOrDefault();
            var isCon = false;

            if (usertype == "3")
            {
                isCon = true;
            }
            else if (id != empID)
            {
                isCon = true;
            }

            if (uID.Equals(Convert.ToInt32(userID)))
            {
                id = empID;
                isCon = false;
            }

            getActiveProjects(id);

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            conn.Open();

            try
            {
                string query = "SELECT e.*, u.Email, CONCAT(e.[State], ', ', e.Country) as Address FROM Employer e LEFT JOIN [User] u on u.UserID = e.UserID WHERE e.EmployerID = '" + id + "'";
                SqlCommand cmd = new SqlCommand(query, conn);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var fname = dr["FirstName"].ToString();
                    var lname = dr["LastName"].ToString();
                    var full = lname + " " + fname;

                    profile.EmployerID = Convert.ToInt32(dr["EmployerID"]);
                    profile.FirstName = full;
                    profile.Email = dr["Email"].ToString();
                    profile.Address = dr["Address"].ToString();
                    profile.Phone = dr["Phone"].ToString();
                    profile.ProfileDesc = dr["ProfileDesc"].ToString();
                    profile.CompanyName = dr["CompanyName"].ToString();
                    profile.isCon = isCon;

                }

                conn.Close();

            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Unsuccesful";
            }

            return View(profile);
        }

        private bool hasActiveProject(int? empID)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            conn.Open();
            //Create command
            string query = "Select Count(*) as Count from Project where EmployerID='"+empID+"' AND [Status]='Active' AND Deleted=0";
            SqlCommand cmd = new SqlCommand(query, conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var count = dr["Count"].ToString();
                if (Convert.ToInt32(count) > 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public void getActiveProjects(int empID)
        {
            if (hasActiveProject(empID))
            {
                SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                SqlDataReader dr;

                conn.Open();
                //Create command
                string query = "SELECT p.Title, p.[Description], p.Cost, p.Duration, (SELECT ABS(DATEDIFF(dd, CURRENT_TIMESTAMP, DatePosted))) as Days, (SELECT ABS(DATEDIFF(hh, CURRENT_TIMESTAMP, DatePosted)))as Hours, (SELECT ABS(DATEDIFF(mi, CURRENT_TIMESTAMP, DatePosted))) as Minutes, c.[Name] as CategoryName, p.SubCategoryName, sl.Skills FROM Project p LEFT JOIN Category c on c.CategoryID = p.CategoryID LEFT JOIN SkillsList sl on sl.ProjectID = p.ProjectID WHERE p.EmployerID = '" + empID + "' AND p.[Status] = 'Active'";
                SqlCommand cmd = new SqlCommand(query, conn);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var day = dr["Days"].ToString();
                    var hr = dr["Hours"].ToString();
                    var min = dr["Minutes"].ToString();
                    string time = min + " minute(s) ago";
                    string skills = dr["Skills"].ToString();
                    string[] array = skills.Split(',');

                    if (Convert.ToInt32(hr) >= 24)
                    {
                        time = day + " day(s) ago";
                    }
                    else if (Convert.ToInt32(min) >= 60)
                    {
                        time = hr + " hour(s) ago";
                    }

                    active.Add(new EmpProfileModel()
                    {
                        Title = dr["Title"].ToString(),
                        Description = dr["Description"].ToString(),
                        Cost = dr["Cost"].ToString(),
                        Duration = dr["Duration"].ToString(),
                        DayHourMin = time,
                        CategoryName = dr["CategoryName"].ToString(),
                        SubCategoryName = dr["SubCategoryName"].ToString(),
                        Skills = array
                    });

                    ViewData["ActiveProject"] = active;
                }

                conn.Close();
            }
            else
            {
                ViewData["ActiveProject"] = "0";
            }


        }

        #endregion



        #region Contractor
        [Route("Contractor/Details/{id}/{uID}")]
        [HttpGet]
        public IActionResult ConDetails(int id, int uID)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var conID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.ContractorID).SingleOrDefault();
            var isCon = true;
            if (uID != Convert.ToInt32(userID))
            {
                var empID = _db.Employer.Where(x => x.UserID.ToString() == userID).Select(x => x.EmployerID).SingleOrDefault();
                return RedirectToAction("EmpDetails", "EmployerDashboard", new { id = empID, uID = 0 });
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
            getPortfolio(id);

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            conn.Open();

            try
            {
                string query = "SELECT c.*, u.Email, CONCAT(c.[State], ', ', c.Country) as [Address], sl.Skills, cat.[Name] as CategoryName FROM Contractor c LEFT JOIN [User] u on u.UserID = c.UserID LEFT JOIN Category cat on cat.CategoryID = c.CategoryID LEFT JOIN SkillsList sl on sl.ContractorID = c.ContractorID WHERE c.ContractorID = '" + id + "' AND c.Deleted = 0";
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
            string query = "SELECT COUNT(*) as [Count] FROM Payment pm  LEFT JOIN Contractor c on c.ContractorID = pm.ContractorID WHERE pm.ContractorID = '" + conID + "' AND c.Deleted=0";
            SqlCommand cmd = new SqlCommand(query, conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var count = dr["Count"].ToString();
                if (Convert.ToInt32(count) > 0)
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
                string query = "SELECT p.Title, p.[Description],p.SubCategoryName, cat.[Name] as CategoryName, sl.Skills FROM Payment pm LEFT JOIN Project p on pm.ProjectID = p.ProjectID LEFT JOIN Category cat on cat.CategoryID = p.CategoryID LEFT JOIN SkillsList sl on sl.ProjectID = p.ProjectID WHERE pm.ContractorID = '" + conID + "'";
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
                string query = "SELECT c.*, u.Email FROM Contractor c LEFT JOIN [User] u on u.UserID = c.UserID WHERE c.ContractorID = '" + conID + "'";
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
                    conProfile.FName = dr["FirstName"].ToString();
                    conProfile.LastName = dr["LastName"].ToString();
                    conProfile.Email = dr["Email"].ToString();
                    conProfile.Phone = dr["Phone"].ToString();
                    conProfile.ProfileDesc = dr["ProfileDesc"].ToString();
                    conProfile.State = dr["State"].ToString();
                    conProfile.Country = dr["Country"].ToString();
                    conProfile.CategoryID = dr["CategoryID"].ToString();
                    conProfile.SubCategoryName = dr["SubCategoryName"].ToString();

                }
            }
            catch (Exception ex)
            {
                TempData["message"] = "Unsuccesful";
            }
            conn.Close();

            return View(conProfile);
        }


        [Route("Contractor/EditDetails/{id}")]
        [HttpPost]
        public IActionResult EditConProfile(int id, ConProfileModel model)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var conID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.ContractorID).SingleOrDefault();

            if (id != conID)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieve ID";
                return RedirectToAction("EmpDetails", new { id = id, uID = userID });
            }

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("Sp_ConUpdateProfile", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {

                cmd.Parameters.AddWithValue("@ConID", conID);
                cmd.Parameters["@ConID"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters["@UserID"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Fname", model.FName);
                cmd.Parameters["@Fname"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Lname", model.LastName);
                cmd.Parameters["@Lname"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@State", model.State);
                cmd.Parameters["@State"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Country", "Malaysia");
                cmd.Parameters["@Country"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Phone", model.Phone);
                cmd.Parameters["@Phone"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Pdesc", model.ProfileDesc);
                cmd.Parameters["@Pdesc"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters["@Email"].Direction = ParameterDirection.Input;


                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Fail to update data!";
                throw ex;
            }
            cmd.Connection.Close();

            return RedirectToAction("ConDetails", new { id = id, uID = userID });
        }

        #endregion



        //[Route("CreateProfile")]
        [HttpPost]
        public IActionResult AddPortfolio(ConProfileModel model)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var conID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.ContractorID).SingleOrDefault();

            string stringFileName = UploadFile(model);

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            try
            {
                conn.Open();
                //Create command
                SqlCommand cmd = new SqlCommand("Sp_AddPortfolio", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ConID", conID);
                cmd.Parameters["@ConID"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@Desc", model.PF_Desc);
                cmd.Parameters["@Desc"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@Title", model.PF_Title);
                cmd.Parameters["@Title"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@Skills", model.PF_Skills);
                cmd.Parameters["@Skills"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@Image", stringFileName);
                cmd.Parameters["@Image"].Direction = ParameterDirection.Input;
               
                cmd.ExecuteNonQuery();


                cmd.Connection.Close();
                TempData["SuccessMsg"] = "Portfolio added successfully";

            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "An error occurred while adding portfolio";
                throw ex;
            }

            return RedirectToAction("ConDetails", new { id = 0, uID = userID });
        }

        private string UploadFile(ConProfileModel model)
        {
            string fileName = null;
            if(model.Portfolio != null)
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "PortfolioImages");
                fileName = Guid.NewGuid().ToString() + "-" + model.Portfolio.FileName;
                string filePath = Path.Combine(uploadDir, fileName);
                using(var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Portfolio.CopyTo(fileStream);
                }
            }
            return fileName;
        }

        private bool hasPortfolio(int? conID)
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            conn.Open();
            //Create command
            string query = "SELECT Count(*) as Count FROM Portfolio WHERE ContractorID = '" + conID + "'";
            SqlCommand cmd = new SqlCommand(query, conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var count = dr["Count"].ToString();
                if (Convert.ToInt32(count) > 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public void getPortfolio(int conID)
        {
            if (hasPortfolio(conID))
            {
                SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                SqlDataReader dr;

                conn.Open();
                //Create command
                string query = "SELECT * FROM Portfolio WHERE ContractorID = '" + conID + "'";
                SqlCommand cmd = new SqlCommand(query, conn);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    string skills = dr["Skills"].ToString();
                    string[] array = skills.Split(',');

                    portfolio.Add(new ConProfileModel()
                    {
                        PF_Title = dr["Title"].ToString(),
                        PF_Desc = dr["Description"].ToString(),
                        PortfolioString =  dr["Image"].ToString(),
                        PF_SkillArray = array,
                    });
                    ViewData["Portfolio"] = portfolio;
                }

                conn.Close();
            }
            else
            {
                ViewData["Portfolio"] = "0";
            }


        }
    }
}
