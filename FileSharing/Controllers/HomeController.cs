using FileSharing.Data;
using FileSharing.Helpers.Mail;
using FileSharing.Models;
using FileSharing.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FileSharing.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext db;
		private readonly IMailHelper mailHelper;
        private readonly IStringLocalizer<SharedResource> localizer;

        private string UserId { get { return User.FindFirstValue(ClaimTypes.NameIdentifier); } }

        public HomeController(ILogger<HomeController> logger,ApplicationDbContext db
            ,IMailHelper mailHelper
            ,IStringLocalizer<SharedResource> localizer)
        {
            _logger = logger;
            this.db = db;
			this.mailHelper = mailHelper;
            this.localizer = localizer;
        }

        public IActionResult Index()
        {
            var highestDownloads =  db.Uploads.OrderByDescending(u => u.DownloadCount)
                .Select(u => new UploadViewModel()
                {
                    FileName = u.FileName,
                    OriginalFileName = u.OriginalFileName,
                    Size = u.Size,
                    ContentType=u.ContentType,
                    DownloadCount=u.DownloadCount,
                    UploadDate=u.UploadDate
                }) .Take(3);
            ViewBag.Popular = highestDownloads;    
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SetCulture(string lang,string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(lang))
            {
                Response.Cookies.Append(
                            CookieRequestCultureProvider.DefaultCookieName,
                            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lang)),
                            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                            );
            }
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Info()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
			if (ModelState.IsValid)
			{
                await db.Contact.AddAsync(new Contact()
                {
                    Name = model.Name,
                    Email=model.Email,
                    Subject= model.Subject,
                    Message=model.Message,
                    UserId=UserId
                });
                await db.SaveChangesAsync();
                TempData["Message"] = localizer["ContactMessage"]?.Value;

                StringBuilder builder = new StringBuilder();

                builder.AppendLine("<h1>File Sharing - Unread Message</h1>");
                builder.AppendFormat("Name : {0}", model.Name);
                builder.AppendFormat("Email : {0}", model.Email);
                builder.AppendLine();
                builder.AppendFormat("Subject : {0}", model.Subject);
                builder.AppendFormat("Message : {0}", model.Message);

                mailHelper.sendMessage(new InputMessageHelper()
                {
                    Subject = "You Have Unread Message",
                    Email = "info@FileSharing.com",
                    Body = builder.ToString()
                });
                return RedirectToAction("Contact");
			}
            return View(model);
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
