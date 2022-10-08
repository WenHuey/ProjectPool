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

        public DbSet<User> User { get; set; }
        //public DbSet<Project> Project { get; set; }
        public DbSet<CreateProjectModel> Project { get; set; }
        public DbSet<SkillsList> SkillsList { get; set; }
        public DbSet<LanguageList> LanguageList { get; set; }


    }
}
