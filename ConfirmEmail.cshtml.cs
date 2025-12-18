// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using L.R._Gcaleka__Co.Data;
using System;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace L.R._Gcaleka__Co.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ConfirmEmailModel> _logger;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager,IEmailSender emailSender, ILogger<ConfirmEmailModel>logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code)
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

            //if(result.Succeeded)
            //{
            //    user.EmployeeNumber = GenerateEmployeeNumber();
            //    await _userManager.UpdateAsync(user);

            //    // Generate Temporary Password
            //    var tempPassword = GenerateTemporaryPassword();
            //    await _userManager.RemovePasswordAsync(user);
            //    await _userManager.AddPasswordAsync(user, tempPassword);

            //    // Send credentials email
            //    await _emailSender.SendEmailAsync(user.Email, "Your Account Details",
            //        $"Your account has been successfully verified. <br>" +
            //        $"Employee Number: <b>{user.EmployeeNumber}</b> <br>" +
            //        $"Temporary Password: <b>{tempPassword}</b> <br>" +
            //        $"Please log in and change your password immediately.");

            //    _logger.LogInformation("User {Email} confirmed their email. Employee Number: {EmployeeNumber} generated.", user.Email, user.EmployeeNumber);

            //    StatusMessage = "Your email has been confirmed. Check your email for login details.";
            //    return RedirectToPage("/Account/ConfirmEmailSuccess");
            //}
            //StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
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
    }
}
