using FileSharing.Data;
using FileSharing.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using System.Security.Claims;
using Microsoft.Extensions.Localization;
using FileSharing.Resources;
using FileSharing.Helpers.Mail;
using System.Text;

namespace FileSharing.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;
        private readonly IStringLocalizer localizer;
        private readonly IMailHelper mailHelper;

        public AccountController(ApplicationDbContext db,
            SignInManager<ApplicationUser> signInManager
            ,UserManager<ApplicationUser> userManager
            ,IMapper mapper
            ,IStringLocalizer<SharedResource> localizer
            ,IMailHelper mailHelper)
        {
            _db = db;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.mapper = mapper;
            this.localizer = localizer;
            this.mailHelper = mailHelper;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
               var existedUser = await userManager.FindByEmailAsync(model.Email);
               if(existedUser == null)
                {
                    TempData["Error"] = "Invalid Email or Password";
                    return View(model);
                }

                if (!existedUser.IsBlocked)
                {
                    var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, true, true);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return LocalRedirect(returnUrl);
                        }
                        return RedirectToAction("Create", "Uploads");
                    }
                    else if (result.IsNotAllowed)
                    {
                        TempData["EmailConfirmErrorMessage"] = localizer["EmaiLConfirmMessage"]?.Value;
                    } 
                }
                else
                {
                    TempData["Error"] = "This Account has been Blocked";
                }
               
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    //Create Link
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var url = Url.Action("ConfirmEmail", "Account", new { token = token , userId = user.Id },Request.Scheme);
                    //Send Email
                    StringBuilder body = new StringBuilder();
                    body.AppendLine("File Sharing Application : Email Confirmation");
                    body.AppendFormat("to confirm your email : you should {0} Click Here ", url);
                    mailHelper.sendMessage(new InputMessageHelper
                    {
                        Body = body.ToString(),
                        Email = model.Email,
                        Subject = "Email Confirmation"
                    });
                    return RedirectToAction("RequireEmailConfirm",new ConfirmEmailViewModel() { 
                       Token = token,
                       UserId = user.Id
                    });
                    //await signInManager.SignInAsync(user,true);
                    //return RedirectToAction("Create", "Uploads");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult RequireEmailConfirm()
        {
            TempData["Message"] = localizer["EmaiLConfirmMessage"]?.Value;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(model.UserId);

                if (user != null)
                {

                    if (!user.EmailConfirmed)
                    {
                        var result = await userManager.ConfirmEmailAsync(user, model.Token);
                        if (result.Succeeded)
                        {
                            TempData["Success"] = localizer["EmailConfirmSuccessDone"]?.Value;
                            return RedirectToAction("Login"); 
                        }
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                    else
                    {
                        TempData["Success"]= localizer["EmailConfirmSuccess"]?.Value;
                    }
                }
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }

        public IActionResult ExternalLogin(string provider)
        {
            var properties =  signInManager.ConfigureExternalAuthenticationProperties(provider, "/Account/ExternalResponse");
            return Challenge(properties,provider);
        }

        public async Task<IActionResult> ExternalResponse()
        {
            var info = await signInManager.GetExternalLoginInfoAsync();
            if(info == null)
            {
                TempData["LoginMessage"] = localizer["LoginMessage"]?.Value;
                return RedirectToAction("Login");
            }
            var loginResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if (!loginResult.Succeeded)
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
                var userToCreate = new ApplicationUser()
                {
                    Email = email,
                    UserName = email,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(userToCreate);
                if (createResult.Succeeded)
                {
                    var exLoginResult = await userManager.AddLoginAsync(userToCreate, info);
                    if (exLoginResult.Succeeded)
                    {
                        await signInManager.SignInAsync(userToCreate, false, info.LoginProvider);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                       await  userManager.DeleteAsync(userToCreate);
                    }
                }
                return RedirectToAction("Login");
            }
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var existedUser = await userManager.FindByEmailAsync(email);
                if (existedUser == null)
                {
                    TempData["Error"] = "Invalid Email or Password ";
                    return RedirectToAction("Login");
                }
                if (existedUser.IsBlocked)
                {
                    await signInManager.SignOutAsync();
                    TempData["Error"] = "This Account has been Blocked";
                    return RedirectToAction("Login");
                }
            }
            return RedirectToAction("Index","Home");
            
        }

        [HttpGet]
        public async Task<IActionResult> Info()
        {
            var currentUser =  await userManager.GetUserAsync(User);
            if(currentUser != null)
            {
                var model = mapper.Map<UserViewModel>(currentUser);
                return View(model);
            }
            return NotFound();
            
        }

        [HttpPost]
        public async Task<IActionResult> Info(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    currentUser.FirstName = model.FirstName;
                    currentUser.LastName = model.LastName;

                    var result = await userManager.UpdateAsync(currentUser);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = localizer["SuccessMessage"]?.Value;
                        return RedirectToAction("Info");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
                else
                {
                    return NotFound();
                }
                
            }
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                if (ModelState.IsValid)
                {
                    var result = await userManager.ChangePasswordAsync(currentUser, model.CurrentPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = localizer["ChangePasswordMessage"]?.Value;
                        await signInManager.SignOutAsync();
                        return RedirectToAction("Login");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            else
            {
                return NotFound();
            }
            return View("Info", mapper.Map<UserViewModel>(currentUser));
        }

        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                if (ModelState.IsValid)
                {
                    var result = await userManager.AddPasswordAsync(currentUser, model.Password);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = localizer["AddPasswordMessage"]?.Value;
                        return RedirectToAction("Info");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            else
            {
                return NotFound();
            }
            return View("Info", mapper.Map<UserViewModel>(currentUser));
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existedUser = await userManager.FindByEmailAsync(model.Email);
                if(existedUser != null)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(existedUser);
                    var url =  Url.Action("ResetPassword", "Account", new { token, model.Email}, Request.Scheme);
                    StringBuilder body = new StringBuilder();
                    body.AppendLine("File Sharing : Reset Password");
                    body.AppendLine("We are sending this email , because we have received a reset password request to your account");
                    body.AppendFormat("to reset a new Password {0} Click this Link", url);
                    mailHelper.sendMessage(new InputMessageHelper()
                    {
                        Body = body.ToString(),
                        Email= model.Email,
                        Subject = "Reset Password"
                    });
                    TempData["Success"] = localizer["SuccessForgotMessage"]?.Value;
                }
                
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(VerifyResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existedUser = await userManager.FindByEmailAsync(model.Email);
                if(existedUser != null)
                {
                    var isValid = await userManager.VerifyUserTokenAsync(existedUser,TokenOptions.DefaultProvider,"ResetPassword",model.Token);
                    if (isValid)
                    {
                        return View();
                    }
                    else
                    {
                        TempData["TokenFailed"] = localizer["TokenFailedMessage"]?.Value;
                    }
                }
                
            }
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existedUser = await userManager.FindByEmailAsync(model.Email);
                if(existedUser != null)
                {
                    var result =await  userManager.ResetPasswordAsync(existedUser,model.Token,model.NewPassword);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = localizer["SuccessResetMessage"]?.Value;
                        return RedirectToAction("Login");
                    }
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);

        }
    }
}


