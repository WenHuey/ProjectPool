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
    public class EmployerDashboardController : Controller
    {
        private readonly DataContext _db;
        private readonly IConfiguration _configuration;
        List<EmpActiveProjectModel> activeProject = new List<EmpActiveProjectModel>();
        List<EmpRunningModel> runningProject = new List<EmpRunningModel>();
        List<EmpApplicationModel> applications = new List<EmpApplicationModel>();
        //List<DashboardModel> projectCount = new List<DashboardModel>();

        public EmployerDashboardController(DataContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        //Active Project
        [Route("Employer/Active")]
        [HttpGet]
        public IActionResult EmpActive()
        {
            //Retrieve userID
            var claimsIdentity = User.Identity as ClaimsIdentity;

            if(claimsIdentity.Claims.Count() == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

            

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
                SqlCommand cmd = new SqlCommand("Sp_DisplayProject", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters["@UserID"].Direction = ParameterDirection.Input;
                cmd.ExecuteNonQuery();

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    activeProject.Add(new EmpActiveProjectModel()
                    {
                        TotalBid = dr["TotalBid"].ToString(),
                        ProjectID = dr["ProjectID"].ToString(),
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

        [Route("Employer/Active/Edit/{id}")]
        public async Task<IActionResult> EditActiveProject(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data";
                return RedirectToAction("EmpActive");
            }

            var getActiveDetails = await _db.Project.FindAsync(id);

            if(getActiveDetails == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data";
                return RedirectToAction("EmpActive");
            }

            SkillsList skills = _db.SkillsList.Find(id);
            if(skills != null)
            {
                getActiveDetails.Skill = skills.Skills;
            }

            LanguageList language = _db.LanguageList.Find(id);
            if (language != null)
            {
                getActiveDetails.Language = language.Language;
            }

            return View(getActiveDetails);
        }

        [Route("Employer/Active/Edit/{id}")]
        [HttpPost]
        public async Task<IActionResult> EditActiveProject(int id, CreateProjectModel model)
        {
            if(id != model.ProjectID)
            {
                return RedirectToAction("EmpActive");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(model);
                    await _db.SaveChangesAsync();

                    if (isSkillExist(id))
                    {
                        if(!EditSkill(model.ProjectID, model.Skill))
                        {
                            ModelState.AddModelError("", "Fail to save skills. Project saved successfully");
                            return View(model);
                        }
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "An error occurred while saving data";
                        return RedirectToAction("EmpActive");
                    }

                    if (isLanguageExist(id))
                    {
                        if(EditLanguage(model.ProjectID, model.Language))
                        {
                            TempData["SuccessMsg"] = "Data successfully saved!";
                            return RedirectToAction("EmpActive");                        
                        }
                        else
                        {
                            ModelState.AddModelError("", "Fail to save language. Project saved successfully");
                            return View(model);
                        }
                    }
                    else
                    {
                        TempData["ErrorMsg"] = "An error occurred while saving data";
                        return RedirectToAction("EmpActive");
                    }
                }
                catch(Exception ex)
                {
                    throw ex;
                    TempData["ErrorMsg"] = "An error occurred while saving data";
                }
            }
            else
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data";
                return RedirectToAction("EmpActive");
            }
        }

        [Route("Employer/Active/Details/{id}")]
        public async Task<IActionResult> ViewActiveProject(int? id)
        {
            if(id == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data";
                return RedirectToAction("EmpActive");
            }

            var getActiveDetails = await _db.Project.FindAsync(id);

            if (getActiveDetails == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data";
                return RedirectToAction("EmpActive");
            }
            else
            {
                SkillsList skillslist = _db.SkillsList.Find(id);
                if (skillslist != null)
                    getActiveDetails.Skill = skillslist.Skills;

                LanguageList languagelist = _db.LanguageList.Find(id);
                if (languagelist != null)
                    getActiveDetails.Language = languagelist.Language;

            }

            return View(getActiveDetails);
        }

        
        [HttpPost]
        public async Task<IActionResult> DeleteActiveProject(int id)
        {
            var project = await _db.Project.FindAsync(id);
            project.Deleted = true;
            await _db.SaveChangesAsync();
            TempData["SuccessMsg"] = "Data successfully deleted!";
            return RedirectToAction("EmpActive");
        }

        [HttpPost]
        public async Task<IActionResult> SetRunningState(int id)
        {
            var project = await _db.Project.FindAsync(id);
            project.Status = "Running";
            await _db.SaveChangesAsync();
            TempData["SuccessMsg"] = "Project status succesfully changed to 'Running'";
            return RedirectToAction("EmpActive");
        }

        //check whether skill exist in db
        private bool isSkillExist(int id)
        {
            return _db.SkillsList.Any(e => e.ProjectID == id);
        }
        
        //update skill to db
        private bool EditSkill(int id, string skill)
        {
            if (isSkillExist(id))
            {
                SkillsList skillslist = _db.SkillsList.Find(id);
                if(skillslist == null)
                {
                    return false;
                }

                skillslist.Skills = skill;
                _db.Update(skillslist);
                _db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        //check whether Language exist in db
        private bool isLanguageExist(int id)
        {
            return _db.LanguageList.Any(e => e.ProjectID == id);
        }

        //update language to db
        private bool EditLanguage(int id, string language)
        {
            if (isLanguageExist(id))
            {
                LanguageList languagelist = _db.LanguageList.Find(id);
                if (languagelist == null)
                {
                    return false;
                }

                languagelist.Language = language;
                _db.Update(languagelist);
                _db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        [Route("Employer/Running")]
        [HttpGet]
        public IActionResult EmpRunning(int projectid)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;

            if (claimsIdentity.Claims.Count() == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

            try
            {
                SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                SqlDataReader dr;

                conn.Open();
                //Create command
                SqlCommand cmd = new SqlCommand("Sp_DisplayRunning", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters["@UserID"].Direction = ParameterDirection.Input;
                cmd.ExecuteNonQuery();

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    runningProject.Add(new EmpRunningModel()
                    {
                        ContractorID = dr["ContractorID"].ToString(),
                        ProjectID = dr["ProjectID"].ToString(),
                        Title = dr["Title"].ToString(),
                        SubCategory = dr["SubCategory"].ToString(),
                        Cost = dr["Cost"].ToString(),
                        FullName = dr["FullName"].ToString(),
                    });
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return View(runningProject);
        }

        [Route("Employer/Application")]
        [HttpGet]
        public IActionResult EmpApplication()
        {
            //Retrieve userID
            
            var claimsIdentity = User.Identity as ClaimsIdentity;
            if (claimsIdentity.Claims.Count() == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

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
                SqlCommand cmd = new SqlCommand("Sp_DisplayApplication", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserID", userID);
                cmd.Parameters["@UserID"].Direction = ParameterDirection.Input;
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
                    else if(Convert.ToInt32(min) >= 60)
                    {
                        time = hr + " hour(s) ago";
                    }



                    applications.Add(new EmpApplicationModel()
                    {
                        ContractorID = dr["ContractorID"].ToString(),
                        EmployerID = dr["EmployerID"].ToString(),
                        ProjectID = dr["ProjectID"].ToString(),
                        FirstName = dr["FirstName"].ToString(),
                        LastName = dr["LastName"].ToString(),
                        Category = dr["Category"].ToString(),
                        SubCategory = dr["SubCategory"].ToString(),
                        Skill = dr["Skills"].ToString(),
                        DayHourMin = time,
                    });
                }


                conn.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return View(applications);
        }

        [HttpGet]
        public IActionResult ReviewApplication()
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
