using ClosedXML.Excel;
using FileSharing.Areas.Admin.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSharing.Areas.Admin
{
    public static class AdminStratup
    {
        public static IServiceCollection AddAdminServices(this IServiceCollection services)
        {
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IXLWorkbook, XLWorkbook>();
            return services;
        }
    }
}
