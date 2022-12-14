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
        ReviewApplicationModel reviewApp = new ReviewApplicationModel();
        List<EmpInterviewModel> interviews = new List<EmpInterviewModel>();
        FinalReviewProjectModel finalReview = new FinalReviewProjectModel();
        List<EmpClosedModel> closedProject = new List<EmpClosedModel>();
        

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
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
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

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
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

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
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

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                return RedirectToAction("ConDashboard", "Dashboard");
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
        public async Task<IActionResult> DeleteActiveProject(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data";
                return RedirectToAction("EmpActive");
            }

            var project = await _db.Project.FindAsync(id);
            project.Deleted = true;
            await _db.SaveChangesAsync();
            TempData["SuccessMsg"] = "Data successfully deleted!";
            return RedirectToAction("EmpActive");
        }


        [HttpPost]
        public IActionResult SetClosedState(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data";
                return RedirectToAction("EmpActive");
            }

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                return RedirectToAction("ConDashboard", "Dashboard");
            }

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            
            try
            {
                conn.Open();
                //Create command
                SqlCommand cmd = new SqlCommand("Sp_SetClosedState", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ProjectID", id);
                cmd.Parameters["@ProjectID"].Direction = ParameterDirection.Input;
                cmd.ExecuteNonQuery();

            
                cmd.Connection.Close();
                TempData["SuccessMsg"] = "Project successfully set to Closed";

            }
            catch(Exception ex)
            {
                TempData["ErrorMsg"] = "An error occurred while closing project";
                throw ex;
            }
            

            
            return RedirectToAction("EmpActive");
        }

        //check whether skill exist in db
        private bool isSkillExist(int? id)
        {
            return _db.SkillsList.Any(e => e.ProjectID == id);
        }
        
        //update skill to db
        private bool EditSkill(int? id, string skill)
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
        private bool isLanguageExist(int? id)
        {
            return _db.LanguageList.Any(e => e.ProjectID == id);
        }

        //update language to db
        private bool EditLanguage(int? id, string language)
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

            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConRunning", "ContractorDashboard");
                }
                
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
                        Progress = dr["Progress"].ToString(),
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

        #region Application

        [Route("Employer/Application")]
        [HttpGet]
        public IActionResult EmpApplication()
        {
            //Retrieve userID
            
            var claimsIdentity = User.Identity as ClaimsIdentity;
            
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConApplication", "ContractorDashboard");
                }
                
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
                        Title = dr["Title"].ToString(),
                        FullName = dr["FullName"].ToString(),
                        Category = dr["Category"].ToString(),
                        SubCategory = dr["SubCategory"].ToString(),
                        ContractorSkills = dr["ContractorSkills"].ToString(),
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

        [Route("Employer/Application/ReviewApplication")]
        [HttpGet]
        public IActionResult ReviewApplication(int? id, int? projectID)
        {
            if (id == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data";
                return RedirectToAction("EmpApplication");
            }

            if (projectID == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving project details";
                return RedirectToAction("EmpApplication");
            }

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
            }

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            conn.Open();

            try
            {
                SqlCommand cmd = new SqlCommand("Sp_GetReviewAppDetails", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@conID", id);
                cmd.Parameters["@conID"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@ProjectID", projectID);
                cmd.Parameters["@ProjectID"].Direction = ParameterDirection.Input;

                dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    var pitch = dr["ConPitch"].ToString();

                    reviewApp.ApplicationID = dr["ApplicationID"].ToString();
                    reviewApp.ProjectID = dr["ProjectID"].ToString();
                    reviewApp.EmployerID = dr["EmployerID"].ToString();
                    reviewApp.ContractorID = dr["ContractorID"].ToString();
                    reviewApp.Title = dr["Title"].ToString();
                    reviewApp.CompanyName = dr["CompanyName"].ToString();
                    reviewApp.Cost = dr["Cost"].ToString();
                    reviewApp.Duration = dr["Duration"].ToString();
                    reviewApp.FullName = dr["FullName"].ToString();
                    reviewApp.Address = dr["Address"].ToString();
                    reviewApp.Email = dr["Email"].ToString();
                    reviewApp.Phone = dr["Phone"].ToString();
                    reviewApp.ProfileDesc = dr["ProfileDesc"].ToString();
                    reviewApp.Tags = dr["Tags"].ToString();
                    reviewApp.Skill = dr["Skill"].ToString();
                    reviewApp.Language = dr["Language"].ToString();
                    reviewApp.ConPitch = pitch;
                    reviewApp.hasPitch = true;
                    if (string.IsNullOrWhiteSpace(pitch))
                    {
                        reviewApp.hasPitch = false;
                    }

                }
            }
            catch
            {
                TempData["ErrorMsg"] = "Error! Data retrieve unsuccesful";
            }
            conn.Close();

            return View(reviewApp);
        }

        [Route("Employer/Application/ReviewApplication")]
        [HttpPost]
        public async Task<IActionResult> ReviewApplication(string id, int value, ReviewApplicationModel model)
        {
            if (id != model.ApplicationID)
            {
                TempData["ErrorMsg"] = "Error! Application Id is Invalid";
                return RedirectToAction("EmpApplication");
            }

            if (value == 0)
            {
                var reject = await _db.Application.FindAsync(id);
                reject.Status = "Reject";
                await _db.SaveChangesAsync();
                return RedirectToAction("EmpApplication");
            }

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
            }

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();
            SqlCommand cmd = new SqlCommand("Sp_InsertInterview", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {

                cmd.Parameters.AddWithValue("@Date", DateTime.Parse(model.IvDate));
                cmd.Parameters["@Date"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@FromTime", DateTime.Parse(model.FromTime));
                cmd.Parameters["@FromTime"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@ToTime", DateTime.Parse(model.ToTime));
                cmd.Parameters["@ToTime"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@ApplicationID", id);
                cmd.Parameters["@ApplicationID"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@Link", model.Link);
                cmd.Parameters["@Link"].Direction = ParameterDirection.Input;

                
                cmd.ExecuteNonQuery();
            }
            catch
            {
                TempData["message"] = "Unsuccesful";
            }
            cmd.Connection.Close();

            return RedirectToAction("EmpApplication");


        }

        #endregion

        #region Interview

        [Route("Employer/Interview")]
        [HttpGet]
        public IActionResult EmpInterview()
        {

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;
            var empID = _db.Employer.Where(x => x.UserID.ToString() == userID).Select(x => x.EmployerID).SingleOrDefault();


            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            conn.Open();
            SqlCommand cmd = new SqlCommand("Sp_DisplayInterview", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {

                cmd.Parameters.AddWithValue("@EmpID", empID);
                cmd.Parameters["@EmpID"].Direction = ParameterDirection.Input;

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var Date = dr["Date"].ToString();
                    var day = dr["Days"].ToString();
                    var hr = dr["Hours"].ToString();
                    var min = dr["Minutes"].ToString();
                    bool datePassed = false;
                    string time = min + " minute(s) more";

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

                    

                    interviews.Add(new EmpInterviewModel()
                    {
                        InterviewID = dr["InterviewID"].ToString(),
                        ApplicationID = dr["ApplicationID"].ToString(),
                        ContractorID = dr["ContractorID"].ToString(),
                        ProjectID = dr["ProjectID"].ToString(),
                        FullName = dr["FullName"].ToString(),
                        Title = dr["Title"].ToString(),
                        Date = DateTime.Parse(Date).ToString("dd/MM/yyyy"),
                        Time = dr["Time"].ToString(),
                        TimeLeft = time,
                        Link = dr["Link"].ToString(),
                        DatePassed = datePassed
                    });
                }
            }
            catch(Exception ex)
            {
                TempData["ErrorMsg"] = "Error! Retrieve Data Unsuccesful";
                throw ex;
            }
            cmd.Connection.Close();

            return View(interviews);
        }

        [Route("Employer/Interview/Edit/{id}")]
        [HttpGet]
        public IActionResult EditInterview(int id)
        {
            if (id == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving key";
                return RedirectToAction("EmpActive");
            }

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
            }

            var getIvDetails = _db.Interview.Where(x => x.InterviewID == id).SingleOrDefault();

            if (getIvDetails == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving interview data";
                return RedirectToAction("EmpInterview");
            }

            return View(getIvDetails);

        }

        [Route("Employer/Interview/Edit/{id}")]
        [HttpPost]
        public async Task<IActionResult> EditInterview(int id, Interview model)
        {
            if(id != model.InterviewID)
            {
                TempData["ErrorMsg"] = "An error occurred! Please seek for customer support.";
                return RedirectToAction("EmpInterview");
            }

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(model);
                    await _db.SaveChangesAsync();

                }
                catch (Exception ex)
                {
                    TempData["ErrorMsg"] = "An error occurred while saving data";
                    throw ex;  
                }
            }
            else
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving data, model state invalid";
                return RedirectToAction("EmpInterview");
            }

            return RedirectToAction("EmpInterview");
        }


        //[Route("Employer/Interview/Approve/{id}")]
        [HttpPost]
        public IActionResult ApproveInterview(int id, int id2)
        {
            if(id == null || id2 == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving ids";
                return RedirectToAction("EmpInterview");
            }

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
            }

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            conn.Open();
            SqlCommand cmd = new SqlCommand("Sp_ApproveInterview", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {

                cmd.Parameters.AddWithValue("@IvID", id);
                cmd.Parameters["@IvID"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@ProjectID", id2);
                cmd.Parameters["@ProjectID"].Direction = ParameterDirection.Input;
                cmd.ExecuteNonQuery();
                
            }
            catch
            {
                TempData["ErrorMsg"] = "Error! Retrieve Data Unsuccesful";
            }

            conn.Close();

            return RedirectToAction("EmpRunning");
        }

        [HttpPost]
        public IActionResult RejectInterview(int id)
        {
            if (id == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieve ID";
                return RedirectToAction("EmpInterview");
            }

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
            }

            //Connect db
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            try
            {
                conn.Open();
                //Create command
                SqlCommand cmd = new SqlCommand("Sp_RejectInterview", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IvID", id);
                cmd.Parameters["@IvID"].Direction = ParameterDirection.Input;

                cmd.ExecuteNonQuery();
                
                conn.Close();
            }
            catch (Exception ex)
            {
                throw ex;
                TempData["ErrorMsg"] = "An error occurred while saving data";
            }
            
            return RedirectToAction("EmpInterview");
        }

        [Route("Employer/Running/FinalReview/{id}")]
        [HttpGet]
        public IActionResult FinalReviewProject(int id, int conID)
        {
            if (id == null || conID == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving Ids";
                return RedirectToAction("EmpRunning");
            }

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
            }

            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            conn.Open();
            SqlCommand cmd = new SqlCommand("Sp_DisplayFinalReview", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            try
            {

                cmd.Parameters.AddWithValue("@ConID", conID);
                cmd.Parameters["@ConID"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@ProjectID", id);
                cmd.Parameters["@ProjectID"].Direction = ParameterDirection.Input;
                //cmd.ExecuteNonQuery();

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    finalReview.ContractorID = conID.ToString();
                    finalReview.ProjectID = id.ToString();
                    //finalReview.EmployerID = dr["EmployerID"].ToString();
                    finalReview.Title = dr["Title"].ToString();
                    finalReview.Scope = dr["Scope"].ToString();
                    finalReview.Tags = dr["Tags"].ToString();
                    finalReview.Cost = dr["Cost"].ToString();
                    finalReview.Duration = dr["Duration"].ToString();
                    finalReview.FullName = dr["FullName"].ToString();
                    finalReview.Email = dr["Email"].ToString();
                    finalReview.Phone = dr["Phone"].ToString();
                    finalReview.FinalAmount = dr["Cost"].ToString();
                    //finalReview.ReviewRate = dr["Cost"].ToString();
                    //finalReview.ReviewDesc = dr["Cost"].ToString();

                }

            }
            catch
            {
                TempData["ErrorMsg"] = "Error! Retrieve Data Unsuccesful";
            }

            conn.Close();

            return View(finalReview);
        }


        [Route("Employer/Running/FinalReview/{id}")]
        [HttpPost]
        public IActionResult FinalReviewProject(int conID, int id, FinalReviewProjectModel model)
        {
            if (id == null || conID == null)
            {
                TempData["ErrorMsg"] = "An error occurred while retrieving Ids";
                return View();
            }

            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

            var empID = _db.Employer.Where(x => x.UserID.ToString() == userID).Select(x => x.EmployerID).SingleOrDefault();

            if (empID != null)
            {
                SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));                SqlDataReader dr;

                conn.Open();
                SqlCommand cmd = new SqlCommand("Sp_InsertPayment", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //try
                //{
                var cRate = model.CompleteRate;
                var pDesc = model.PaymentDesc;
                
                if(cRate == "1")
                {
                    cRate = "100%";
                }
                else if(cRate == "0.7")
                {
                    cRate = "> 50%";
                }
                else if (cRate == "0.4"){
                    cRate = "< 50%";
                }

                    
                cmd.Parameters.AddWithValue("@ContractorID", conID);
                cmd.Parameters["@ContractorID"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@EmployerID", empID);
                cmd.Parameters["@EmployerID"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@Amount", model.FinalAmount);
                cmd.Parameters["@Amount"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@Desc", model.PaymentDesc == null ? DBNull.Value.ToString() : model.PaymentDesc);
                cmd.Parameters["@Desc"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@ProjectID", id);
                cmd.Parameters["@ProjectID"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@CompleteRate", cRate);
                cmd.Parameters["@CompleteRate"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@ReviewDesc", model.ReviewDesc == null ? DBNull.Value.ToString() : model.ReviewDesc);
                cmd.Parameters["@ReviewDesc"].Direction = ParameterDirection.Input;
                cmd.Parameters.AddWithValue("@Rating", model.ReviewRate == null ? "0" : model.ReviewRate);
                cmd.Parameters["@Rating"].Direction = ParameterDirection.Input;
                cmd.ExecuteNonQuery();

                //}
                //catch
                //{
                //    TempData["ErrorMsg"] = "Error! Retrieve Data Unsuccesful";
                //}

                conn.Close();
            }
            else
            {
                TempData["ErrorMsg"] = "Error! Fail to retrieve empID";
            }

            

            return RedirectToAction("EmpRunning");
        }

        #endregion


        public IActionResult EmpClosed()
        {
            //Retrieve userID
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var usertype = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (usertype != "2")
            {
                if (usertype == "1") //admin
                {
                    return RedirectToAction("AdminProjectList", "Admin");
                }
                else if (usertype == "3") //contractor
                {
                    return RedirectToAction("ConDashboard", "Dashboard");
                }
            }

            var userID = claimsIdentity.FindFirst(ClaimTypes.Sid).Value;

            var empID = _db.Employer.Where(x => x.UserID.ToString() == userID).Select(x => x.EmployerID).SingleOrDefault();


            //Connect db
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;

            if (closedProject.Count > 0)
            {
                closedProject.Clear();
            }
            try
            {
                conn.Open();
                //Create command
                SqlCommand cmd = new SqlCommand("Sp_DisplayClosed", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@EmpID", empID);
                cmd.Parameters["@EmpID"].Direction = ParameterDirection.Input;
                cmd.ExecuteNonQuery();

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var Date = dr["PaymentDate"].ToString();
                    var cRate = dr["CompletionRate"].ToString();
                    var amount = dr["Amount"].ToString();
                    var fullName = dr["FullName"].ToString();
                    var status = "Closed";

                    if (!string.IsNullOrWhiteSpace(Date) || !string.IsNullOrWhiteSpace(cRate) || !string.IsNullOrWhiteSpace(amount) || !string.IsNullOrWhiteSpace(fullName))
                    {
                        status = "Completed";
                    }

                    if (string.IsNullOrWhiteSpace(Date))
                    {
                        Date = "-";
                    }
                    else
                    {
                        Date = DateTime.Parse(Date).ToString("dd/MM/yyyy hh:mm f");
                    }

                    if (string.IsNullOrWhiteSpace(cRate))
                    {
                        cRate = "-";
                    }
                    if (string.IsNullOrWhiteSpace(amount))
                    {
                        amount = "-";
                    }
                    if (string.IsNullOrWhiteSpace(fullName))
                    {
                        fullName = "-";
                    }



                    closedProject.Add(new EmpClosedModel()
                    {
                        
                        ProjectID = dr["ProjectID"].ToString(),
                        Title = dr["Title"].ToString(),
                        Cost = dr["Cost"].ToString(),
                        CompletionRate = cRate,
                        Amount = amount,
                        PaymentDate = Date,
                        FullName = fullName,
                        Status = status
                    });
                }
                conn.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return View(closedProject);
        }


        



    }
}
