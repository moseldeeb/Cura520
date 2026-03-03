using AspNetCoreGeneratedDocument;
using Cura520.Models;
using Cura520.Repos;
using Cura520.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Threading.Tasks;

namespace Cura520.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;

        public AccountController(
            UserManager<ApplicationUser> userManager ,
            SignInManager<ApplicationUser> signInManager ,
            IEmailSender emailSender,
            IRepository<ApplicationUserOTP> applicationUserOTPRepository
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOTPRepository = applicationUserOTPRepository;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register() 
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }
            ApplicationUser user = new ApplicationUser()
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
            };  
            var result = await _userManager.CreateAsync(user , registerVM.Password); 
            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(registerVM);
            }
            var token  = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail) , "Account" ,new {Area = "Identity" ,token, userId = user.Id} , Request.Scheme);
            await _emailSender.SendEmailAsync(registerVM.Email, "Ecommerce 520 Confirm Email",
                $"<h1> confirm your email by clicking <a href='{link}'> here</a>  </h1>"); 
            return RedirectToAction("Login");
        }
        public async Task<IActionResult> ConfirmEmail(string token , string userId)
        {
            var  user  = await _userManager.FindByIdAsync(userId);
            if (user is null )
            {
                TempData["Error"] = "Invalid User";
            }
            var result  = await _userManager.ConfirmEmailAsync(user , token);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Email Confirmation Failed";
            }
            else
            {
                TempData["Success"] = "Email Confirmed Successfully";
            }
             return RedirectToAction("Login");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVm loginVm)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVm);
            }
            var user  =  await _userManager.FindByNameAsync(loginVm.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(loginVm.UserNameOrEmail);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                return View(loginVm);
            }
            //await _userManager.CheckPasswordAsync(user , loginVm.Password); 
            var result = await _signInManager.PasswordSignInAsync(user, loginVm.Password , loginVm.RememberMe , true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Your account is locked out. Please try again later.");
                    return View(loginVm);
                }
                else if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "You need to confirm your email before logging in.");
                    return View(loginVm);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                    return View(loginVm);
                }
            }
            return RedirectToAction("Index" , "Home" , new { area = "Customer" });
        }
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation( ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            var user  =  await _userManager.FindByNameAsync(resendEmailConfirmationVM.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(resendEmailConfirmationVM.UserNameOrEmail);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid UserName Or Email");
                return View(resendEmailConfirmationVM);
            }
            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Email is already confirmed");
                return View(resendEmailConfirmationVM);
            }
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { Area = "Identity", token, userId = user.Id }, Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email, "Ecommerce 520 Confirm Email",
                $"<h1> confirm your email by clicking <a href='{link}'> here</a>  </h1>");
            return RedirectToAction("Login");

        }
        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPassword)
        {
            var user = await _userManager.FindByNameAsync(forgetPassword.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(forgetPassword.UserNameOrEmail);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid UserName Or Email");
                return View(forgetPassword);
            }
            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Confirm your Email first");
                return View(forgetPassword);
            }
            var otps = await _applicationUserOTPRepository.GetAsync(opt=>opt.ApplicationUserId == user.Id);
            var last12hoursOTP = otps.Count(otp=> otp.CreatedAt > DateTime.UtcNow.AddHours(-24) ); 
            if (last12hoursOTP >= 10)
            {
                ModelState.AddModelError(string.Empty, "You have exceeded the maximum number of OTP requests. Please try again later.");
                return View(forgetPassword);
            }
            foreach(var otp in otps)
            {
                 otp.IsValid = false;
                 _applicationUserOTPRepository.Update(otp);
                 await _applicationUserOTPRepository.CommitAsync();
            }
            var OTP =  new Random().Next(1000 , 9999).ToString();
            ApplicationUserOTP applicationUserOTP = new ApplicationUserOTP(OTP, user.Id); 
             await _applicationUserOTPRepository.AddAsync(applicationUserOTP);
            await _applicationUserOTPRepository.CommitAsync();
            await _emailSender.SendEmailAsync(user.Email, "Ecommerce 520 Reset Password",
                $"<h1> use this OTP <span style ='color:red;'>'{OTP}'</span> to Rest your Password  </h1>");
            return RedirectToAction("ValidateOTP" , new{ applicationUserId = user.Id });
        }
        public IActionResult ValidateOTP( string applicationUserId )
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ValidateOTP(ValidateOTPVM validateOTPVM)
        {
            var user = await  _userManager.FindByIdAsync(validateOTPVM.ApplicationUserId);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid user");
                return View(validateOTPVM);
            }
            var applicationUserOTP = await _applicationUserOTPRepository.GetOneAsync(
                otp => otp.ApplicationUserId == validateOTPVM.ApplicationUserId && 
                otp.OTP == validateOTPVM.OTP &&
                otp.IsValid 
                );
            if (applicationUserOTP is null )
            {
                ModelState.AddModelError(string.Empty, "Invalid OTP");
                return View(validateOTPVM);
            }
            if (DateTime.UtcNow > applicationUserOTP.ExpireAt)
            {
                applicationUserOTP.IsValid = false;
                _applicationUserOTPRepository.Update(applicationUserOTP);
                await _applicationUserOTPRepository.CommitAsync();
                ModelState.AddModelError(string.Empty, "OTP Expired");
                return View(validateOTPVM);
            }
            applicationUserOTP.IsValid = false;
            _applicationUserOTPRepository.Update(applicationUserOTP);
            await _applicationUserOTPRepository.CommitAsync();
            TempData["OTPValidateUerId"] = user.Id;
            return RedirectToAction("ResetPassword", new { applicationUserId = user.Id } );

        }
        public IActionResult ResetPassword(string applicationUserId)
        {
            var OTPValidateUerId = TempData["OTPValidateUerId"]?.ToString();
            if (String.IsNullOrEmpty(OTPValidateUerId) || OTPValidateUerId != applicationUserId)
            {
                ModelState.AddModelError(string.Empty, "Unauthorized Access to Reset Password");
                return RedirectToAction("ForgetPassword");
            }
            TempData.Keep("OTPValidateUerId");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVM)
        {
            var OTPValidateUerId = TempData["OTPValidateUerId"]?.ToString();
            if (String.IsNullOrEmpty(OTPValidateUerId) || OTPValidateUerId != resetPasswordVM.ApplicationUserId)
            {
                ModelState.AddModelError(string.Empty, "Unauthorized Access to Reset Password");
                return RedirectToAction("ForgetPassword");
            }
            var user = await _userManager.FindByIdAsync(resetPasswordVM.ApplicationUserId);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid user");
                return View(resetPasswordVM);
            }
            var token  = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user , token, resetPasswordVM.NewPassword);
            if(!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(resetPasswordVM);
            }
            TempData.Remove("OTPValidateUerId");
            return RedirectToAction("Login");

        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index" , "Home" , new { area = "Customer" });
        }
        [HttpGet]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


    }
}
