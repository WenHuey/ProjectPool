using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ProjectPool.Models
{
    public class DataContext: DbContext
    {
        public DataContext(DbContextOptions<DataContext> options):base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            optionsBuilder.UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);
        }

        public DataSet GetProjectTitle(int id)
        {
            SqlConnection conn = new SqlConnection("Data Source=WENHUEY\\SQLEXPRESS;Initial Catalog=ProjectPool;Integrated Security=True;");
            SqlCommand cmd = new SqlCommand("Sp_DisplayRunningTitle", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserID", id);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            da.Fill(ds);
            return ds;
        }

        public DbSet<User> User { get; set; }
        //public DbSet<Project> Project { get; set; }
        public DbSet<CreateProjectModel> Project { get; set; }
        public DbSet<SkillsList> SkillsList { get; set; }
        public DbSet<LanguageList> LanguageList { get; set; }
        public DbSet<ApplicationModel> Application { get; set; }

        public DbSet<Contractor> Contractor { get; set; }
    }
}
