using FileSharing.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileSharing.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessageResourceName ="Required",ErrorMessageResourceType =typeof(SharedResource))]
        [Display(Name = "NameLabel", ResourceType = typeof(SharedResource))]
        public string Name { get; set; }
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
        [EmailAddress(ErrorMessageResourceName ="Email",ErrorMessageResourceType =typeof(SharedResource))]
        [Display(Name = "EmailLabel", ResourceType = typeof(SharedResource))]
        public string  Email { get; set; }
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
        [Display(Name = "SubjectLabel", ResourceType = typeof(SharedResource))]
        public string Subject { get; set; }
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(SharedResource))]
        [Display(Name = "MessageLabel", ResourceType = typeof(SharedResource))]
        public string Message { get; set; }
    }
}
