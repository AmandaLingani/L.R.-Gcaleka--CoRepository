using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using L.R._Gcaleka__Co.Data;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Net.Mail;
using L.R._Gcaleka__Co.Models;


namespace L.R._Gcaleka__Co.Areas.Identity.Pages.Account
{
    public class RegisterClientModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public RegisterClientModel(
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

        [BindProperty]
        public ClientInputModel ClientInput { get; set; }
        public string ReturnUrl { get; set; } 
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class ClientInputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }
            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }
        public async Task OnGetAsync(string returnUrl=null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, ClientInput.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, ClientInput.Email, CancellationToken.None);

                user.FirstName = ClientInput.FirstName;
                user.LastName = ClientInput.LastName;

                var result = await _userManager.CreateAsync(user, ClientInput.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Client");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await SendEmailAsync(user.Email, "Confirm Your Email",
                    $"Your account with L.R. Gcaleka & Co has been created. <br>" +
                    $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.<br><br>" +
                   
                    $"You cannot log in until your email is confirmed.");

                    return RedirectToPage("/Account/RegisterConfirmation", new { email = ClientInput.Email, returnUrl = returnUrl });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return Page();
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
        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                   $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                   $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
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
