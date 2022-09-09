using Microsoft.AspNetCore.Identity;
using ProjectPool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectPool.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AccountRepository(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        //public async Task CreateAccountAsync(SignUpModel signUpModel)
        //{
        //    var user = new IdentityUser()
        //    {
                

        //    }
        //    //_userManager.CreateAsync
        //}
    }
}
