using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Data
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ProjectPoolContext(serviceProvider.GetRequiredService<DbContextOptions<ProjectPoolContext>>()))
            {
                //Look for Sign Up
                if (context.SignUpModel.Any()) { return; }


            }
        }
    }
}
