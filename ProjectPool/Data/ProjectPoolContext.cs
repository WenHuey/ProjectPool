using Microsoft.EntityFrameworkCore;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Data
{
    public class ProjectPoolContext : DbContext
    {
        public ProjectPoolContext(DbContextOptions<ProjectPoolContext> options)
            : base(options)
        {

        }

        public DbSet<SignUpModel> SignUpModel { get; set; }
    }
}
