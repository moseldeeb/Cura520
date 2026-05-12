using Cura520.Models;
using Cura520.Models;
using Cura520.Repos;
using Cura520.Utilities;
using Cura520.ViewModel.Identity;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cura520.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;
        private readonly ILogger<AccountController> _logger;
        private readonly IRepository<Models.Patient> _patientRepository;

        public AccountController(
            UserManager<ApplicationUser> userManager ,
            SignInManager<ApplicationUser> signInManager ,
            IEmailSender emailSender,
            IRepository<ApplicationUserOTP> applicationUserOTPRepository,
            ILogger<AccountController> logger,
            IRepository<Models.Patient> patientRepository
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOTPRepository = applicationUserOTPRepository;
            _logger = logger;
            _patientRepository = patientRepository;
        }

        public IActionResult Register() 
        {
            return View(new SimpleRegisterVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(SimpleRegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            try
            {
                // Auto-generate username from email (first part before @)
                var username = registerVM.Email.Split('@')[0];
                
                // Check if username already exists
                var existingUser = await _userManager.FindByEmailAsync(registerVM.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered. Please use a different email.");
                    return View(registerVM);
                }

                // Create ApplicationUser with minimal required info
                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = registerVM.Email,
                    FirstName = registerVM.FirstName,
                    LastName = registerVM.LastName,
                    Type = UserType.Patient,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, registerVM.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(registerVM);
                }

                // Assign Patient role
                await _userManager.AddToRoleAsync(user, SD.Role_Patient);

                // Create minimal Patient profile (only required fields + user reference)
                var patient = new Models.Patient
                {
                    FirstName = registerVM.FirstName,
                    LastName = registerVM.LastName,
                    ApplicationUserId = user.Id,
                    // Other fields left empty for later profile completion
                    Gender = string.Empty,
                    PhoneNumber = string.Empty,
                    BloodType = string.Empty,
                    Allergies = string.Empty
                };

                await _patientRepository.AddAsync(patient);
                await _patientRepository.CommitAsync();

                // Generate email confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var link = Url.Action(nameof(ConfirmEmail), "Account",
                    new { Area = "Identity", token, userId = user.Id }, Request.Scheme);

                // Send confirmation email
                await _emailSender.SendEmailAsync(registerVM.Email, "Cura 520 - Confirm Email",
                    $"<h1>Welcome to Cura!</h1>" +
                    $"<p>Please confirm your email by clicking <a href='{link}'>here</a></p>");

                TempData["Success"] = "Registration successful! Please check your email to confirm your account.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during registration: {ex.Message}");
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(registerVM);
            }
        }
        
        public async Task<IActionResult> ConfirmEmail(string token, string userId)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Invalid confirmation link";
                return RedirectToAction("Login");
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                TempData["Error"] = "Invalid user";
                return RedirectToAction("Login");
            }
            
            if (user.EmailConfirmed)
            {
                TempData["Info"] = "Email already confirmed";
                return RedirectToAction("Login");
            }
            
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Email confirmation failed. " + 
                    string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction("ResendEmailConfirmation");
            }
            
            TempData["Success"] = "Email confirmed successfully! You can now login.";
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
            // Redirect based on user role
            if (await _userManager.IsInRoleAsync(user, SD.Role_SuperAdmin))
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            else if (await _userManager.IsInRoleAsync(user, SD.Role_Admin))
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            else if (await _userManager.IsInRoleAsync(user, SD.Role_Doctor))
            {
                return RedirectToAction("Index", "Home", new { area = "Doctor" });
            }
            else if (await _userManager.IsInRoleAsync(user, SD.Role_Patient))
            {
                // For patients, check if profile is complete
                // First-time patients go to profile completion page
                return RedirectToAction("CompleteProfile", "Account", new { area = "Identity" });
            }
            else if (await _userManager.IsInRoleAsync(user, SD.Role_Receptionist))
            {
                return RedirectToAction("Index", "Home", new { area = "Receptionist" });
            }
            else
            {
                // Default to Patient area if no role is assigned
                return RedirectToAction("CompleteProfile", "Account", new { area = "Identity" });
            }
        }
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            if (!ModelState.IsValid)
            {
                return View(resendEmailConfirmationVM);
            }
            
            var user = await _userManager.FindByNameAsync(resendEmailConfirmationVM.UserNameOrEmail) 
                ?? await _userManager.FindByEmailAsync(resendEmailConfirmationVM.UserNameOrEmail);
            
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, 
                    "No account found with that username or email address.");
                return View(resendEmailConfirmationVM);
            }
            
            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, 
                    "Your email is already confirmed. You can login now.");
                return View(resendEmailConfirmationVM);
            }
            
            // Generate new email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", 
                new { Area = "Identity", token, userId = user.Id }, Request.Scheme);
            
            // Send confirmation email
            await _emailSender.SendEmailAsync(user.Email, "Cura 520 - Confirm Email",
                $"<h1>Email Confirmation Required</h1>" +
                $"<p>Please confirm your email by clicking <a href='{link}'>here</a></p>" +
                $"<p>If you didn't request this, please ignore this email.</p>");
            
            TempData["Success"] = "Confirmation email has been sent. Please check your inbox.";
            return RedirectToAction("Login");
        }
        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPassword)
        {
            if (!ModelState.IsValid)
            {
                return View(forgetPassword);
            }
            
            var user = await _userManager.FindByNameAsync(forgetPassword.UserNameOrEmail) 
                ?? await _userManager.FindByEmailAsync(forgetPassword.UserNameOrEmail);
            
            if (user is null)
            {
                // Don't reveal if the account exists for security reasons
                TempData["Info"] = "If an account exists with that username or email, an OTP will be sent.";
                return RedirectToAction("Login");
            }
            
            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, 
                    "Please confirm your email first before resetting password. " +
                    "Use 'Resend Email Confirmation' option.");
                return View(forgetPassword);
            }
            
            // Check OTP request limits (max 10 requests in 24 hours)
            var otps = await _applicationUserOTPRepository.GetAsync(
                opt => opt.ApplicationUserId == user.Id);
            
            var last24hoursOTP = otps.Count(
                otp => otp.CreatedAt > DateTime.UtcNow.AddHours(-24));
            
            if (last24hoursOTP >= 10)
            {
                ModelState.AddModelError(string.Empty, 
                    "You have exceeded the maximum number of OTP requests (10 per 24 hours). " +
                    "Please try again later.");
                return View(forgetPassword);
            }
            
            // Invalidate all previous valid OTPs
            var validOtps = otps.Where(o => o.IsValid).ToList();
            foreach(var otp in validOtps)
            {
                otp.IsValid = false;
                _applicationUserOTPRepository.Update(otp);
                await _applicationUserOTPRepository.CommitAsync();
            }
            
            // Generate new OTP (6-digit for security) using cryptographically secure random
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                byte[] tokenData = new byte[4];
                rng.GetBytes(tokenData);
                int randomNumber = Math.Abs(BitConverter.ToInt32(tokenData, 0)) % 1000000;
                var OTP = randomNumber.ToString("D6");
                ApplicationUserOTP applicationUserOTP = new ApplicationUserOTP(OTP, user.Id);
                
                await _applicationUserOTPRepository.AddAsync(applicationUserOTP);
                await _applicationUserOTPRepository.CommitAsync();
                
                // Send OTP email
                await _emailSender.SendEmailAsync(user.Email, "Cura 520 - Password Reset OTP",
                    $"<h1>Password Reset Request</h1>" +
                    $"<p>Your OTP for password reset is: <strong style='font-size: 24px; color: #d4351b;'>{OTP}</strong></p>" +
                    $"<p>This OTP is valid for 10 minutes only.</p>" +
                    $"<p>If you didn't request this, please ignore this email and your password will remain unchanged.</p>");
            }
            
            TempData["Success"] = "OTP has been sent to your email. Please check your inbox.";
            return RedirectToAction("ValidateOTP", new { applicationUserId = user.Id });
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
            return RedirectToAction("Index" , "Home" , new { area = "Patient" });
        }
        [HttpGet]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// GET: Patient profile completion page (called after first login)
        /// </summary>
        [Authorize(Roles = "Patient")]
        [HttpGet]
        public async Task<IActionResult> CompleteProfile()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                var patient = await _patientRepository.GetOneAsync(p => p.ApplicationUserId == user.Id && !p.IsDeleted);
                if (patient == null)
                {
                    return RedirectToAction("Logout");
                }

                var profileVM = new PatientProfileVM
                {
                    PatientId = patient.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    DateOfBirth = patient.DateOfBirth != DateTime.MinValue ? patient.DateOfBirth : null,
                    Gender = patient.Gender,
                    PhoneNumber = patient.PhoneNumber,
                    BloodType = patient.BloodType,
                    Allergies = patient.Allergies,
                    Address = user.Address,
                    ProfilePhotoUrl = patient.Img,
                    IsProfileComplete = !string.IsNullOrEmpty(patient.PhoneNumber) && patient.DateOfBirth != DateTime.MinValue
                };

                return View(profileVM);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading profile completion page: {ex.Message}");
                TempData["Error"] = "An error occurred. Please try again.";
                return RedirectToAction("Index", "Home", new { area = "Patient" });
            }
        }

        /// <summary>
        /// POST: Update patient profile information
        /// </summary>
        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteProfile(PatientProfileVM profileVM)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                var patient = await _patientRepository.GetOneAsync(p => p.ApplicationUserId == user.Id && !p.IsDeleted);
                if (patient == null)
                {
                    return RedirectToAction("Logout");
                }

                // Update patient profile with form data
                patient.DateOfBirth = profileVM.DateOfBirth ?? DateTime.MinValue;
                patient.Gender = profileVM.Gender ?? string.Empty;
                patient.PhoneNumber = profileVM.PhoneNumber ?? string.Empty;
                patient.BloodType = profileVM.BloodType ?? string.Empty;
                patient.Allergies = profileVM.Allergies ?? string.Empty;

                // Update address in ApplicationUser
                user.Address = profileVM.Address ?? string.Empty;

                // Handle profile photo upload
                if (profileVM.ProfilePhoto != null && profileVM.ProfilePhoto.Length > 0)
                {
                    try
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileVM.ProfilePhoto.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Patients", fileName);

                        var directory = Path.GetDirectoryName(filePath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await profileVM.ProfilePhoto.CopyToAsync(stream);
                        }

                        patient.Img = fileName;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error uploading profile photo: {ex.Message}");
                        ModelState.AddModelError("ProfilePhoto", "Error uploading profile photo. Please try again.");
                        return View(profileVM);
                    }
                }

                // Update patient record
                _patientRepository.Update(patient);
                await _patientRepository.CommitAsync();

                // Update user record
                var userUpdateResult = await _userManager.UpdateAsync(user);
                if (!userUpdateResult.Succeeded)
                {
                    ModelState.AddModelError("", "Error updating user information.");
                    return View(profileVM);
                }

                TempData["Success"] = "Profile completed successfully! You can now access all features.";
                return RedirectToAction("Index", "Home", new { area = "Patient" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating profile: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating your profile. Please try again.");
                return View(profileVM);
            }
        }

        /// <summary>
        /// AJAX: Skip profile completion and go to dashboard
        /// </summary>
        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> SkipProfileCompletion()
        {
            try
            {
                // Just redirect to patient home - user can complete profile later
                return RedirectToAction("Index", "Home", new { area = "Patient" });
            }
            catch
            {
                return RedirectToAction("Index", "Home", new { area = "Patient" });
            }
        }


    }
}
