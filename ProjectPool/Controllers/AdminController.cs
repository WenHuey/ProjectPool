using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProjectPool.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly DataContext _db;
        private readonly IConfiguration _configuration;
        List<ProjectListModel> allProject = new List<ProjectListModel>();
        List<AdminDashboardModel> allEmployer = new List<AdminDashboardModel>();
        List<AdminDashboardModel> allContractor = new List<AdminDashboardModel>();

        public AdminController(DataContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [Route("Admin/Login")]
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

        [AllowAnonymous]
        [Route("Admin/Login")]
        [HttpPost]
        public async Task<IActionResult> Index(AdminLoginModel model)
        {
            if(model.Email == null || model.Password == null)
            {
                ViewBag.error = "Error occured. Please try again.";
                return View();
            }
            else if(model.Email == "admin@gmail.com" && model.Password == "123qwe")
{
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Sid, "5")
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                    //IsPersistent = user.Keep
                };

                var userID = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), properties);

                
                return RedirectToAction("AdminProjectList");

            }
            else
            {
                ViewBag.error = "Invalid email or password.";
                return View();
            }

        }


        //View all project
        [Route("Admin/ProjectListing")]
        [HttpGet]
        public IActionResult AdminProjectList(AdminLoginModel model)
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (usertype != "1")
                {
                    if (usertype == "2") //employer
                    {
                        return RedirectToAction("EmpDashboard", "Dashboard");
                    }
                    else if (usertype == "3") //contractor
                    {
                        return RedirectToAction("ConDashboard", "Dashboard");
                    }
                }
            }

            getAnalytic();

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            conn.Open();

            try
            {
                string query = "Select p.ProjectID, p.Title, p.[Status], CONCAT(em.LastName, ' ', em.FirstName) as EmpName FROM Project p LEFT JOIN Employer em on em.EmployerID = p.EmployerID WHERE p.Deleted = 0";
                SqlCommand cmd = new SqlCommand(query, conn);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    allProject.Add(new ProjectListModel()
                    {
                        ProjectID = dr["ProjectID"].ToString(),
                        Title = dr["Title"].ToString(),
                        Status = dr["Status"].ToString(),
                        EmpName = dr["EmpName"].ToString()
                    });
                }
            }
            catch
            {
                TempData["ErrorMsg"] = "Unsuccesful in retrieving data.";
            }
            conn.Close();

            return View(allProject);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteProject(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data";
                return RedirectToAction("AdminProjectList");
            }

            var project = await _db.Project.FindAsync(id);
            project.Deleted = true;
            await _db.SaveChangesAsync();
            TempData["SuccessMsg"] = "Project successfully deleted!";
            return RedirectToAction("AdminProjectList");
        }


        public void getAnalytic()
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            conn.Open();
            //Create command
            string query = "SELECT(SELECT Count(*) FROM Project WHERE Deleted = 0) as ProjectCount,(SELECT Count(*) FROM Employer WHERE Deleted = 0) as EmployerCount,(SELECT Count(*) FROM Contractor WHERE Deleted = 0) as ContractorCount";
            SqlCommand cmd = new SqlCommand(query, conn);
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                var emp = dr["EmployerCount"].ToString();
                var con = dr["ContractorCount"].ToString();
                var user = Convert.ToInt32(emp) + Convert.ToInt32(con);

                var totalCount = new AdminDashboardModel()
                {                  
                    ProjectCount = dr["ProjectCount"].ToString(),
                    UserCount = user.ToString(),
                    EmployerCount = emp,
                    ContractorCount = con,
                };
                ViewData["TotalCount"] = totalCount;
            }

            conn.Close();

        }

        //View all employers
        [Route("Admin/EmployerListing")]
        [HttpGet]
        public IActionResult AdminEmployerList(AdminLoginModel model)
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (usertype != "1")
                {
                    if (usertype == "2") //employer
                    {
                        return RedirectToAction("EmpDashboard", "Dashboard");
                    }
                    else if (usertype == "3") //contractor
                    {
                        return RedirectToAction("ConDashboard", "Dashboard");
                    }
                }
            }

            getAnalytic();

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            conn.Open();

            try
            {
                string query = "SELECT EmployerID, UserID, FirstName, LastName, CompanyName FROM Employer WHERE Deleted = 0";
                SqlCommand cmd = new SqlCommand(query, conn);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    allEmployer.Add(new AdminDashboardModel()
                    {
                        EmployerID = dr["EmployerID"].ToString(),
                        Emp_FN = dr["FirstName"].ToString(),
                        Emp_LN = dr["LastName"].ToString(),
                        Emp_CN = dr["CompanyName"].ToString(),
                        UserID = dr["UserID"].ToString(),
                    });
                }
            }
            catch
            {
                TempData["ErrorMsg"] = "Unsuccesful in retrieving data.";
            }
            conn.Close();

            return View(allEmployer);
        }

        //View all employers
        [Route("Admin/ContractorListing")]
        [HttpGet]
        public IActionResult AdminContractorList(AdminLoginModel model)
        {

            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (usertype != "1")
                {
                    if (usertype == "2") //employer
                    {
                        return RedirectToAction("EmpDashboard", "Dashboard");
                    }
                    else if (usertype == "3") //contractor
                    {
                        return RedirectToAction("ConDashboard", "Dashboard");
                    }
                }
            }

            getAnalytic();

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            conn.Open();

            try
            {
                string query = "SELECT ContractorID, UserID, FirstName, LastName, [State], [Country] FROM Contractor WHERE Deleted = 0";
                SqlCommand cmd = new SqlCommand(query, conn);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var state = dr["State"].ToString();
                    var country = dr["Country"].ToString();
                    var location = "Not specified";
                    
                    if(!string.IsNullOrWhiteSpace(state) && !string.IsNullOrWhiteSpace(country))
                    {
                        location = state + ", " + country;
                    }
                    else if (string.IsNullOrWhiteSpace(state) && string.IsNullOrWhiteSpace(country))
                    {
                        location = "Not specified";
                    }
                    else if (string.IsNullOrWhiteSpace(state))
                    {
                        location = country;
                    }
                    else if (string.IsNullOrWhiteSpace(country))
                    {
                        location = state;
                    }


                    allContractor.Add(new AdminDashboardModel()
                    {
                        ContractorID = dr["ContractorID"].ToString(),
                        UserID = dr["UserID"].ToString(),
                        Con_FN = dr["FirstName"].ToString(),
                        Con_LN = dr["LastName"].ToString(),
                        Con_Location = location
                    });
                }
            }
            catch
            {
                TempData["ErrorMsg"] = "Unsuccesful in retrieving data.";
            }
            conn.Close();

            return View(allContractor);
        }

    }
}
