using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class ProjectController : Controller
    {
        private readonly DataContext _db;
        private readonly IConfiguration _configuration;
        List<ProjectListModel> allProject = new List<ProjectListModel>();

        public ProjectController(DataContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [Route("ProjectList")]

        public IActionResult ProjectList()
        {

            //Connect db
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
                SqlCommand cmd = new SqlCommand("Sp_DisplayAllProject", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.ExecuteNonQuery();

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

                    string str = dr["Description"].ToString();



                    allProject.Add(new ProjectListModel()
                    {
                        ProjectID = dr["ProjectID"].ToString(),
                        Title = dr["Title"].ToString(),
                        Description = dr["Description"].ToString(),
                        Duration = dr["Duration"].ToString(), 
                        Status = dr["Status"].ToString(),
                        Cost = dr["Cost"].ToString(),
                        CategoryName = dr["CategoryName"].ToString(), 
                        SubCategoryName = dr["SubCategoryName"].ToString(), 
                        PostedAgo = time,
                        TotalBid = dr["TotalBid"].ToString()
                        
                    });
                }
                conn.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return View(allProject);



            //ClaimsPrincipal claimUser = HttpContext.User;
            //if (claimUser.Identity.IsAuthenticated)
            //{
            //    var claimsIdentity = User.Identity as ClaimsIdentity;

            //    if (claimsIdentity.Claims.Count() == 0)
            //    {
            //        return RedirectToAction("Login", "Account");
            //    }

            //    var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            //    var getAllProject = await _db.get;


            //    return View(getAllProject);
            //}
                

            //return RedirectToAction("Login", "Account");
        }



        [Route("ProjectDetail/{id}")]
        [HttpGet]
        public IActionResult ProjectDetail(string id, ProjectListModel model)
        {
            model.ProjectID = id;   
            return View(model);
        }

        
        [Route("ProjectDetail/{id}")]
        [HttpPost]
        public IActionResult ProjectDetail(ProjectListModel model)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var contractorID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("Sp_Createproject", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {

                cmd.Parameters.AddWithValue("@ProjectID", model.ProjectID);
                cmd.Parameters["@Title"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@ContractorID", contractorID);
                cmd.Parameters["@Description"].Direction = ParameterDirection.Input;

               
                cmd.ExecuteNonQuery();
            }
            catch
            {
                TempData["message"] = "Unsuccesful";
            }
            cmd.Connection.Close();

            return View(model);
        }


        [Route("CreateProject")]
        public IActionResult CreateProject()
        {
            //int count = 0;
            //string[] skills;
            ////Connect db
            //string connStr = _configuration.GetConnectionString("DefaultConnection");
            //SqlConnection conn = new SqlConnection(connStr);

            //string query = "Insert into Employer (FirstName, LastName, Email, [Password]) VALUES (@FirstName, @LastName, @Email, hashbytes('sha2_512', @Password))";
            //SqlCommand cmd = new SqlCommand(query, conn);


            //String str = "select c_name from contacts where user_id = " + user_id + "";
            //MySqlCommand cmd = new MySqlCommand(str, dbConnection);
            //MySqlDataReader mdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);

            //List<string> list = new List<string>();
            //while (mdr.Read())
            //{
            //    list.Add(mdr.GetString(0));
            //}

            //string[] strMyArray = list.ToArray<string>();

            return View();
        }

        
        [Route("CreateProject")]
        [HttpPost]
        public IActionResult CreateProject(CreateProjectModel model, string tag)
        {

            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("Sp_Createproject", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {
                
                cmd.Parameters.AddWithValue("@Title", model.Title);
                cmd.Parameters["@Title"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Description", model.Description);
                cmd.Parameters["@Description"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Duration", model.Duration);
                cmd.Parameters["@Duration"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Cost", model.Cost);
                cmd.Parameters["@Cost"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters["@UserID"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Skill", model.Skill);
                cmd.Parameters["@Skill"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Language", model.Language);
                cmd.Parameters["@Language"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@CategoryID", model.CategoryID);
                cmd.Parameters["@CategoryID"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@SubCategoryName", model.SubCategoryName);
                cmd.Parameters["@SubCategoryName"].Direction = ParameterDirection.Input;
                
                cmd.ExecuteNonQuery();
            }
            catch
            {
                TempData["message"] = "Unsuccesful";
            }
            cmd.Connection.Close();

            //Back to active product page
            return RedirectToAction("EmpActive", "EmployerDashboard");
        }



        //public async Task<IActionResult> EditProject(int? id)
        //{
        //    if(id == null)
        //    {
        //        return RedirectToAction();
        //    }
        //}
    }
}
