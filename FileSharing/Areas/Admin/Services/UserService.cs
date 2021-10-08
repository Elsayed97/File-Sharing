using AutoMapper;
using AutoMapper.QueryableExtensions;
using FileSharing.Areas.Admin.Models;
using FileSharing.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSharing.Areas.Admin.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext db;
        private readonly IMapper mapper;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserService(ApplicationDbContext db
            , IMapper mapper
            ,UserManager<ApplicationUser> userManager
            ,RoleManager<IdentityRole> roleManager)
        {
            this.db = db;
            this.mapper = mapper;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        public async Task<OperationResult> ToogleBlockUserAsync(string UserId)
        {
            var existedUser = await db.Users.FindAsync(UserId);
            if (existedUser == null)
            {
                return OperationResult.NotFound();
            }
            existedUser.IsBlocked = !existedUser.IsBlocked;
            db.Update(existedUser);
            await db.SaveChangesAsync();
            return OperationResult.Succeded();
        }

        public IQueryable<AdminUserViewModel> GetAll()
        {
            var result = db.Users
                .OrderByDescending(u => u.CreatedDate)
                .ProjectTo<AdminUserViewModel>(mapper.ConfigurationProvider);
            return result;
        }

        public IQueryable<AdminUserViewModel> GetBlockedUsers()
        {
            var result = db.Users.Where(u => u.IsBlocked)
                .OrderByDescending(u => u.CreatedDate)
                .ProjectTo<AdminUserViewModel>(mapper.ConfigurationProvider);
            return result;
        }

        public async Task<int> UserRegisterationCountAsync()
        {
            var count = await db.Users.CountAsync();
            return count;
        }

        public async Task<int> UserRegisterationCountAsync(int month)
        {
            var year = DateTime.Today.Year;
            var count = await db.Users.CountAsync(u => u.CreatedDate.Month==month && u.CreatedDate.Year ==year);
            return count;
        }

        public IQueryable<AdminUserViewModel> Search(string term)
        {
            var result = db.Users.Where(u => u.Email == term || 
                         u.FirstName.Contains(term) || u.LastName.Contains(term)
                         || (u.FirstName + " " + u.LastName).Contains(term))
                        .OrderByDescending(u => u.CreatedDate)
                        .ProjectTo<AdminUserViewModel>(mapper.ConfigurationProvider);
            return result;
        }

        public async Task InitializeAsync()
        {
            if(!await roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole()
                {
                    Name = UserRoles.Admin
                });
            }
            var adminEmail = "admin@a.com";
            if(await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var user = new ApplicationUser()
                {
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "05046641258"
                };
                await userManager.CreateAsync(user,"P@ssw0rd");

                await userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
        }
    }
}
