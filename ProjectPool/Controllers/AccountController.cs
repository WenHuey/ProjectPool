using Microsoft.AspNetCore.Mvc;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace ProjectPool.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController (IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("Signup")]
        public IActionResult SignUp()
        {
            return View();
        }

        [Route("Signup")]
        [HttpPost]
        public IActionResult SignUp(SignUpModel model)
        {
            //Connect db
            string connStr = _configuration.GetConnectionString("DefaultConnection");
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();

            //Create command
            string query = "Insert into Employer (FirstName, LastName, Email, [Password]) VALUES (@FirstName, @LastName, @Email, hashbytes('sha2_512', @Password))";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
            cmd.Parameters.AddWithValue("@LastName", model.LastName);
            cmd.Parameters.AddWithValue("@Email", model.Email);
            cmd.Parameters.AddWithValue("@Password", model.Password);

            //Execute query
            cmd.ExecuteNonQuery();

            //close connection
            conn.Close();

            return View("Login");
        }


        [Route("/")]
        public IActionResult Login()
        {
            return View();
        }

        //[Route("/")]
        //[HttpPost]
        //public IActionResult Login()
        //{
        //    return View();
        //}
    }
}
