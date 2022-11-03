using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Controllers
{
    [Authorize]
    public class ContractorController : Controller
    {
        private readonly DataContext _db;
        private readonly IConfiguration _configuration;
        List<ContractorListModel> conList = new List<ContractorListModel>();

        public ContractorController(DataContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [Route("ContractorList")]
        public IActionResult ContractorList()
        {
            SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataReader dr;
            conn.Open();

            try
            {
                string query = "SELECT c.ContractorID, CONCAT(c.LastName, ' ', c.FirstName) as FullName, c.[State], c.Country, c.ProfileDesc, c.ReviewAverage, c.SubCategoryName ,cat.[Name] as CategoryName FROM Contractor c LEFT JOIN Category cat on cat.CategoryID = c.CategoryID WHERE c.Deleted = 0";
                SqlCommand cmd = new SqlCommand(query, conn);

                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var state = dr["State"].ToString();
                    var country = dr["Country"].ToString();
                    var address = "Not specified";
                    var reviewAvg = dr["ReviewAverage"].ToString();
                    var subCat = dr["SubCategoryName"].ToString();
                    var hasReview = true;
                    var hasSub = true;

                    if (string.IsNullOrWhiteSpace(subCat))
                    {
                        hasSub = false;
                    }

                    if (!string.IsNullOrWhiteSpace(state) && !string.IsNullOrWhiteSpace(country))
                    {
                        address = state + ", " + country;
                    }
      

                    if(double.Parse(reviewAvg) < 3.5)
                    {
                        hasReview = false;
                    }


                    conList.Add(new ContractorListModel()
                    {
                        ContractorID = dr["ContractorID"].ToString(),
                        Name = dr["FullName"].ToString(),
                        Address = address,
                        ProfileDesc = dr["ProfileDesc"].ToString(),
                        ReviewAverage = dr["ReviewAverage"].ToString(),
                        SubCategoryName = dr["SubCategoryName"].ToString(),
                        CategoryName = dr["CategoryName"].ToString(),
                        HasReview = hasReview,
                        HasSub = hasSub,
                    });
                }

            }
            catch (Exception ex)
            {
                TempData["message"] = "Unsuccesful";
                throw ex;
            }

            return View(conList);
        }
    }
}
