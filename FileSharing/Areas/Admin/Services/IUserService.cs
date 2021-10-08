using FileSharing.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSharing.Areas.Admin.Services
{
    public interface IUserService
    {
        IQueryable<AdminUserViewModel> GetAll();
        IQueryable<AdminUserViewModel> GetBlockedUsers();
        IQueryable<AdminUserViewModel> Search(string term);
        Task<OperationResult> ToogleBlockUserAsync(string UserId);
        Task<int> UserRegisterationCountAsync();
        Task<int> UserRegisterationCountAsync(int month);

        Task InitializeAsync();

    }
}
