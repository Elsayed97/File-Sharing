using FileSharing.Resources;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileSharing.Models
{
    public class InputFile
    {
        [Required]
        public IFormFile File { get; set; }
    }

    public class InputUpload
    {
        public string OriginalFileName { set; get; }
        public string FileName { set; get; }
        public string ContentType { set; get; }
        public long Size { set; get; }
        public string UserId { set; get; }
    }
    public class UploadViewModel
    {
        [Display(Name = "OriginalFileNameLabel",ResourceType =typeof(SharedResource))]
        public string OriginalFileName { get; set; }
        [Display(Name = "FileNameLabel",ResourceType =typeof(SharedResource))]
        public string FileName { get; set; }

        [Display(Name ="SizeLabel",ResourceType =typeof(SharedResource))]
        public decimal Size { get; set; }

        [Display(Name = "ContentTypeLabel", ResourceType = typeof(SharedResource))]
        public string ContentType { get; set; }

        [Display(Name = "UploadDateLabel", ResourceType = typeof(SharedResource))]
        public DateTime UploadDate { get; set; }
        public string Id { get; set; }
        [Display(Name = "DownloadCountLabel", ResourceType = typeof(SharedResource))]
        public long DownloadCount { get; set; }
    }

}
