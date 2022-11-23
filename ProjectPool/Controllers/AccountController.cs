using Microsoft.AspNetCore.Mvc;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ProjectPool.Service;
//using ProjectPool.Models;

namespace ProjectPool.Controllers
{
    public class AccountController : Controller
    {
        //private DataContext db = new DataContext();
        private readonly DataContext _db;

        private readonly IConfiguration _configuration;

        public AccountController (DataContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [Route("ChooseUser")]
        public IActionResult UserType()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;
                var userID = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                if (userID != null)
                {
                    if (userID == "2") //employer
                    {
                        return RedirectToAction("EmpDashboard", "Dashboard");
                    }
                    else if (userID == "3") //contractor
                    {
                        return RedirectToAction("ConDashboard", "Dashboard");
                    }
                }
            }
                
            return View();
        }

        [Route("ChooseUser")]
        [HttpPost]
        public IActionResult UserType(string usertypeID)
        {
            TempData["typeID"] = usertypeID;
            return RedirectToAction("SignUp");
        }
        
        //check if user exist in database
        public JsonResult IsUserExist(string email)
        {
            System.Threading.Thread.Sleep(200);
            if (email == null)
            {
                return Json(2);
            }

            var data = _db.User.Where(e => e.Email.ToLower() == email.ToLower()).SingleOrDefault();
            if(data == null)
            {
                return Json(0);
            }
            else
            {
                return Json(1);
            }
        }

        //[Route("ChooseUser/Signup")]
        [HttpGet]
        public IActionResult SignUp(int typeID)
        {
            ViewBag.typeID = Convert.ToInt32(TempData["typeID"]);

            return View();
        }

        //[Route("ChooseUser/Signup")]
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpModel model)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);


            //Connect db
            string connStr = _configuration.GetConnectionString("DefaultConnection");
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            //create command
            SqlCommand cmd = new SqlCommand("Sp_Signup", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {
                if (model.UserTypeID != null)
                {

                    //hash password
                    model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    //add value
                    cmd.Parameters.AddWithValue("@UserTypeID", model.UserTypeID);
                    cmd.Parameters["@UserTypeID"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                    cmd.Parameters["@FirstName"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@LastName", model.LastName);
                    cmd.Parameters["@LastName"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters["@Email"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@Password", model.Password);
                    cmd.Parameters["@Password"].Direction = ParameterDirection.Input;
                }

                //Execute query
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //close connection
            conn.Close();

            return RedirectToAction("Login", "Account");
            //return Json(new { redirectUrl = Url.Action("Login", "Account") });

        }


        [Route("/")]
        [Route("Account/Login")]
        public IActionResult Login()
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

        [Route("/")]
        [Route("Account/Login")]
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = checkUser(email, password);
            string name;
            if (user == null)
            {
                ViewBag.error = "Invalid email or password";
                //bool userExist
                return View();
            }
            else
            {
                //Connect db
                string connStr = _configuration.GetConnectionString("DefaultConnection");
                SqlConnection conn = new SqlConnection(connStr);
                conn.Open();
                //create command
                SqlCommand cmd = new SqlCommand("Sp_Login", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    cmd.Parameters.AddWithValue("@UserTypeID", user.UserTypeID);
                    cmd.Parameters["@UserTypeID"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@UserID", user.UserID);
                    cmd.Parameters["@UserID"].Direction = ParameterDirection.Input;
                    cmd.Parameters.Add("@Output", SqlDbType.NVarChar, 50);
                    cmd.Parameters["@Output"].Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    name = (string)cmd.Parameters["@Output"].Value;
                    HttpContext.Session.SetString("name", name);
                    conn.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                //user.Email = name;
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.NameIdentifier, user.UserTypeID.ToString()),
                    new Claim(ClaimTypes.Sid, user.UserID.ToString())
                    //new Claim(ClaimTypes.Role, "Administrator"),
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

                if (userID != null)
                {
                    if (userID == "2") //employer
                    {
                        return RedirectToAction("EmpDashboard", "Dashboard");
                    }
                    else if (userID == "3") //contractor
                    {
                        return RedirectToAction("ConDashboard", "Dashboard");
                    }
                }

                

                //return View("~/Views/Dashboard/Dashboard.cshtml");
                //return RedirectToAction("Dashboard", "Dashboard");
                return RedirectToAction("Welcome");
            }
            
        }

        private User checkUser(string email, string password)
        {
            var user = _db.User.SingleOrDefault(x => x.Email.Equals(email));
            if (user != null)
            {
                if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    return user;
                }
            }
            return null;
        }

        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("name");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        //Testing use
        [Route("Welcome")]
        [Authorize]
        public IActionResult Welcome()
        {
            //ClaimsPrincipal claimUser = HttpContext.User;

            //if (claimUser.Identity.IsAuthenticated)
            //{
            return View();
            //}

            //return RedirectToAction("Login");

        }

        
    }
}
