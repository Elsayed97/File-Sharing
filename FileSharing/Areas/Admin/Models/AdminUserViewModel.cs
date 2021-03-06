using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSharing.Areas.Admin.Models
{
    public class AdminUserViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedDate { get; set; }

        public string UserId { get; set; }
    }
}
