using Microsoft.EntityFrameworkCore;
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
    }
}
