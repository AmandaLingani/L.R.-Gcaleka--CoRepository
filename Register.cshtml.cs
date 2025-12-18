// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using L.R._Gcaleka__Co.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using L.R._Gcaleka__Co.Data;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;

namespace L.R._Gcaleka__Co.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }
        
        //public List<SelectListItem> RoleList { get; set; } = new List<SelectListItem>()
        //{
        //    new SelectListItem{Value="Admin", Text="Admin"},
        //    new SelectListItem{Value="Candidate Attorney", Text="Candidate Attorney"},
        //    new SelectListItem{Value="CEO", Text="CEO"},
        //    //new SelectListItem{Value="Client", Text="Client"}
        //};

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }
            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Role")]
            public string Role { get; set; }

            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }

        }
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            Input = new InputModel()
            {
                RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
                {
                    Text = i,
                    Value = i
                })
            };
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            Console.WriteLine("OnPostAsync has been triggered");
            _logger.LogInformation("OnPostAsync has been triggered!");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid");
                _logger.LogInformation("ModelState is invalid");
                foreach (var key in ModelState.Keys)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        Console.WriteLine($"Validation Error in {key}:{error.ErrorMessage}");
                        _logger.LogWarning($"Validation Error in {key}:{error.ErrorMessage}");
                    }
                    return Content("ModelState is invalid. Fix the errors.");
                }

                return Content("ModelState is Valid! Proceeding...");
            }
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            string generatedPassword = GenerateTemporaryPassword();

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                EmployeeNumber = GenerateEmployeeNumber(),
            };
            //var user = CreateUser();


            var result = await _userManager.CreateAsync(user, generatedPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation("Admin registered a new employee account");

                await _userManager.AddToRoleAsync(user, Input.Role);

                var userId = await _userManager.GetUserIdAsync(user);

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Confirm Your Email",
                    $"Your account with L.R. Gcaleka & Co has been created. <br>" +
                    $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<br><br>" +
                    $"Your login details: <br>" +
                    $"Employee Number: <b>{user.EmployeeNumber}</b><br>" +
                    $"Password: <b>{generatedPassword}</b><br>" +
                    $"You cannot log in until your email is confirmed.");

                return RedirectToPage("/Account/RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private string GenerateEmployeeNumber()
        {
            var random = new Random();
            return $"LGC-{random.Next(1000, 9999)}";
        }

        private string GenerateTemporaryPassword()
        {
            return $"Gcalekalr{Guid.NewGuid().ToString().Substring(0, 9)}!";
        }

        public async Task<IActionResult> OnGetConfirmEmailAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {

                await _userManager.UpdateAsync(user);

                return RedirectToPage("/Account/ConfirmEmailSuccess");
            }

            return RedirectToPage("Account/ConfirmEmailFailure");
        }
        private async Task SendEmailAsync(string to, string subject, string body)
        {

            var smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
            using (SmtpClient smtp = new SmtpClient(smtpSettings.Server, smtpSettings.Port))
            {
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;


                MailMessage message = new MailMessage
                {
                    To = { to },
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    //From = new MailAddress("lrgcaleka.Co@gmail.com")
                };
                message.To.Add(to);
                try
                {
                    await smtp.SendMailAsync(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email sending failed: {ex.Message}");
                }
            }
        }
        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
