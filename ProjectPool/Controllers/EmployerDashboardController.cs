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
    public class EmployerDashboardController : Controller
    {
        private readonly DataContext _db;
        private readonly IConfiguration _configuration;
        List<EmpActiveProjectModel> activeProject = new List<EmpActiveProjectModel>();
        //List<DashboardModel> projectCount = new List<DashboardModel>();

        public EmployerDashboardController(DataContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        //Active Project
        [Route("Employer/Active")]
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
        public async Task<IActionResult> EditActiveProject1(int? id)
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
        public async Task<IActionResult> EditActiveProject1(int id, CreateProjectModel model)
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
                return RedirectToAction("EmpActive");
            }
        }

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

            return View(getActiveDetails);
        }

        private bool isSkillExist(int id)
        {
            return _db.SkillsList.Any(e => e.ProjectID == id);
        }

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

        private bool isLanguageExist(int id)
        {
            return _db.LanguageList.Any(e => e.ProjectID == id);
        }

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


        [Route("EmpRunningProject")]
        public IActionResult EmpRunningProject()
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
