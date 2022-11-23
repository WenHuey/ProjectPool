using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class ProjectController : Controller
    {
        private readonly DataContext _db;
        private readonly IConfiguration _configuration;
        List<ProjectListModel> allProject = new List<ProjectListModel>();
        ProjectDetailsModel projectDetails = new ProjectDetailsModel();

        public ProjectController(DataContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [Route("ProjectList")]
        [HttpGet]
        public IActionResult ProjectList(string searchInput, int val, int ddval)
        {

            ClaimsPrincipal claimUser = HttpContext.User;
           
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
                SqlCommand cmd;

                if (claimUser.Identity.IsAuthenticated) {
                    var claimsIdentity = User.Identity as ClaimsIdentity;
                    var usertypeID = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                    
                    if(usertypeID == "3" && val != 1)
                    {
                        var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
                        var conID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.ContractorID).SingleOrDefault();
                        var catID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.CategoryID).SingleOrDefault();
                        var subName = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.SubCategoryName).SingleOrDefault();
                        var skill = _db.SkillsList.Where(x => x.ContractorID.Equals(conID)).Select(x => x.Skills).SingleOrDefault();

                        if(catID != null || catID != 0 || !string.IsNullOrWhiteSpace(subName))
                        {
                            cmd = new SqlCommand("Sp_DisplayAllProjectFiltered", conn);

                            cmd.Parameters.AddWithValue("@CatID", catID);
                            cmd.Parameters["@CatID"].Direction = ParameterDirection.Input;
                            cmd.Parameters.AddWithValue("@SubName", subName == null ? DBNull.Value.ToString() : subName);
                            cmd.Parameters["@SubName"].Direction = ParameterDirection.Input;
                            cmd.Parameters.AddWithValue("@SkillList", skill == null ? DBNull.Value.ToString() : skill);
                            cmd.Parameters["@SkillList"].Direction = ParameterDirection.Input;
                        }
                        else
                        {
                            cmd = new SqlCommand("Sp_DisplayAllProject", conn);
                            cmd.Parameters.AddWithValue("@SearchInput", searchInput == null ? DBNull.Value.ToString() : searchInput);
                            cmd.Parameters["@SearchInput"].Direction = ParameterDirection.Input;
                            cmd.Parameters.AddWithValue("@Category", ddval);
                            cmd.Parameters["@Category"].Direction = ParameterDirection.Input;
                        }

                    }
                    else
                    {
                        cmd = new SqlCommand("Sp_DisplayAllProject", conn);
                        cmd.Parameters.AddWithValue("@SearchInput", searchInput == null ? DBNull.Value.ToString() : searchInput);
                        cmd.Parameters["@SearchInput"].Direction = ParameterDirection.Input;
                        cmd.Parameters.AddWithValue("@Category", ddval);
                        cmd.Parameters["@Category"].Direction = ParameterDirection.Input;

                    }
                }
                else 
                {
                    cmd = new SqlCommand("Sp_DisplayAllProject", conn);
                    cmd.Parameters.AddWithValue("@SearchInput", searchInput == null ? DBNull.Value.ToString() : searchInput);
                    cmd.Parameters["@SearchInput"].Direction = ParameterDirection.Input;
                    cmd.Parameters.AddWithValue("@Category", ddval);
                    cmd.Parameters["@Category"].Direction = ParameterDirection.Input;
                }

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
                        TotalBid = dr["TotalBid"].ToString(),
                        Skills = dr["Skills"].ToString(),

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

        private bool isApplied(int? projectID, int conID)
        {
            return _db.Application.Any(e => e.ProjectID == projectID && e.ContractorID == conID);
        }

        [Route("ProjectDetail/{id}")]
        [HttpGet]
        public IActionResult ProjectDetail(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data";
                return RedirectToAction("ProjectList");
            }
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var conID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.ContractorID).SingleOrDefault();
            try
            {
                SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                SqlDataReader dr;

                conn.Open();
                //Create command
                SqlCommand cmd = new SqlCommand("Sp_GetProjectDetails", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ProjectID", id);
                cmd.Parameters["@ProjectID"].Direction = ParameterDirection.Input;
                cmd.ExecuteNonQuery();

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    //projectDetails.Add(new ProjectDetailsModel()
                    //{
                    var Date = dr["DatePosted"].ToString();
                    
                    projectDetails.Status = dr["Status"].ToString();
                    projectDetails.EmployerID = dr["EmployerID"].ToString();
                    projectDetails.FullName = dr["FullName"].ToString();
                    projectDetails.Phone = dr["Phone"].ToString();
                    projectDetails.Email = dr["Email"].ToString();
                    projectDetails.CompanyName = dr["CompanyName"].ToString();
                    projectDetails.TotalBid = dr["TotalBid"].ToString();
                    projectDetails.ProjectID = dr["ProjectID"].ToString();
                    projectDetails.Title = dr["Title"].ToString();
                    projectDetails.Description = dr["Description"].ToString();
                    projectDetails.Scope = dr["Scope"].ToString();
                    projectDetails.Duration = dr["Duration"].ToString();
                    projectDetails.Cost = dr["Cost"].ToString();
                    projectDetails.DatePosted = DateTime.Parse(Date).ToString("dd/MM/yyyy");
                    projectDetails.SubCategoryName = dr["SubCategoryName"].ToString();
                    projectDetails.CategoryName = dr["CategoryName"].ToString();
                    projectDetails.Skills = dr["Skills"].ToString();
                    projectDetails.Language = dr["Language"].ToString();
                    projectDetails.isApplied = false;
                    projectDetails.isEmp = false;

                    if (usertype == "2")
                    {
                        projectDetails.isEmp = true;
                    }
                    //});
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (isApplied(id, conID))
            {
                projectDetails.isApplied = true;
            }


            return View(projectDetails);
        }


        //[Route("ProjectDetail/{id}")]
        //[HttpPost]
        //public IActionResult ProjectDetail(ProjectListModel model)
        //{
        //    var claimsIdentity = User.Identity as ClaimsIdentity;
        //    var contractorID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

        //    SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        //    conn.Open();
        //    SqlCommand cmd = new SqlCommand("Sp_Createproject", conn);
        //    cmd.CommandType = CommandType.StoredProcedure;

        //    try
        //    {

        //        cmd.Parameters.AddWithValue("@ProjectID", model.ProjectID);
        //        cmd.Parameters["@Title"].Direction = ParameterDirection.Input;

        //        cmd.Parameters.AddWithValue("@ContractorID", contractorID);
        //        cmd.Parameters["@Description"].Direction = ParameterDirection.Input;


        //        cmd.ExecuteNonQuery();
        //    }
        //    catch
        //    {
        //        TempData["message"] = "Unsuccesful";
        //    }
        //    cmd.Connection.Close();

        //    return View(model);
        //}


        [Route("ApplyProject/{id}")]
        [HttpGet]
        public IActionResult ApplyProject(int id)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

            //var conID = _db.Contractor.FromSqlRaw("Select * From Contractor Where UserID = '"+userID+"'");
            var conID = _db.Contractor.SingleOrDefault(x => x.UserID.ToString() == userID);
            //var conID = _db.Contractor.FirstOrDefault(x => x.UserID.ToString() == userID);

            if (conID.Phone == null || conID.ProfileDesc == null || conID.CategoryID == null || conID.SubCategoryName == null)
            {
                TempData["message"] = "Please complete profile before applying for project.";
                return RedirectToAction("EditConProfile","Profile", new { id = conID.ContractorID });
            }

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            conn.Open();

            

            try
            {
                string query = "SELECT p.ProjectID, p.Title, e.CompanyName, p.Cost, p.Duration FROM Project p LEFT JOIN Employer e on e.EmployerID = p.EmployerID WHERE p.ProjectID ='"+id+"'";
                SqlCommand cmd = new SqlCommand(query, conn);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {

                    projectDetails.ProjectID = dr["ProjectID"].ToString();
                    projectDetails.Title = dr["Title"].ToString();
                    projectDetails.CompanyName = dr["CompanyName"].ToString();
                    projectDetails.Duration = dr["Duration"].ToString();
                    projectDetails.Cost = dr["Cost"].ToString();

                }
            }
            catch
            {
                TempData["message"] = "Unsuccesful";
            }
            conn.Close();

            return View(projectDetails);
        }

        [Route("ApplyProject/{id}")]
        [HttpPost]
        public IActionResult ApplyProject(int? id, ProjectDetailsModel model)
        {
            if(model.ProjectID == null)
            {
                TempData["ErrorMsg"] = "Error occured! Please seek for customer support. ";
                return RedirectToAction("ProjectDetail");
            }

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

            var conID = _db.Contractor.Where(x => x.UserID.ToString() == userID).Select(x => x.ContractorID).SingleOrDefault();

            try
            {
                SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                SqlDataReader dr;

                conn.Open();
                //Create command
                SqlCommand cmd = new SqlCommand("Sp_InsertApplication", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ProjectID", id);
                cmd.Parameters["@ProjectID"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@ContractorID", conID);
                cmd.Parameters["@ContractorID"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@Pitch", model.Pitch == null ? DBNull.Value.ToString() : model.Pitch);
                cmd.Parameters["@Pitch"].Direction = ParameterDirection.Input;
                
                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = "Error occured! Insert Unsuccesful.";
                return RedirectToAction("ApplyProject/{id}", id);
            }
            return RedirectToAction("ConApplication", "ContractorDashboard");
        }

        [Route("CreateProject")]
        public IActionResult CreateProject()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

            var empID = _db.Employer.SingleOrDefault(x => x.UserID.ToString() == userID);

            if (empID.Phone == null || empID.ProfileDesc == null || empID.CompanyName == null )
            {
                TempData["message"] = "Please complete profile before posting project.";
                return RedirectToAction("EditEmpProfile", "Profile", new { id = empID.EmployerID });
            }

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

                cmd.Parameters.AddWithValue("@Scope", model.Scope);
                cmd.Parameters["@Scope"].Direction = ParameterDirection.Input;

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
