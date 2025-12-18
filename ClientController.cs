using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using L.R._Gcaleka__Co.Models;
using L.R._Gcaleka__Co.Data;
using L.R._Gcaleka__Co.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Google.Cloud.Storage.V1;
using Hangfire;
using System.Net;
using System.Net.Mail;
using System;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace L.R._Gcaleka__Co.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly string _bucketName = "lrgcaleka-uploads";
        private readonly StorageClient _storageClient;
        private readonly CloudStorageService _storageService;
        private readonly IEmailSender _emailSender;
        public ClientController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<ClientController> logger, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, IUserStore<ApplicationUser> userStore, IEmailSender emailSender)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userStore = userStore;
            _storageClient = StorageClient.Create();
            _storageService = new CloudStorageService();
            _emailSender = emailSender;
        }

        public IActionResult ClientHome()
        {
            return View();
        }

        public IActionResult Clients()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ScheduleAppointment()
        {
            var clientele = await _userManager.GetUserAsync(User);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleAppointment(AppointmentViewModel viewModel)
        {
            var clientele = await _userManager.GetUserAsync(User);
            if (clientele == null)
            {
                return NotFound("User not found, make sure you are registered");
            }

            var model = new Appointment
            {
                AppointmentId = Guid.NewGuid().ToString(),
                AppointmentDate = viewModel.AppointmentDate,
                AppointmentType = viewModel.AppointmentType,
                ContactNumber = viewModel.ContactNumber,
                ClientId = clientele.Id,
                ReminderSent = false
            };
            _context.Appointment.Add(model);
            await _context.SaveChangesAsync();

            await _emailSender.SendEmailAsync(clientele.Email, "Appointment Confirmation",
                $"Your appointment for {model.AppointmentDate} has been successfully booked.");

            var reminderTime = model.AppointmentDate.AddMinutes(-30); //executes -30mins before appointment
            BackgroundJob.Schedule<ReminderService>(
                service => service.SendAppointmentReminders(
                model.AppointmentId), reminderTime);

            return RedirectToAction("ScheduleAppointment");
        }

        [HttpGet]
        public async Task<IActionResult> UploadDocument()
        {
            var client = await _userManager.GetUserAsync(User); //Who's uploading here
            if (client == null)
            {
                return Unauthorized("User not found.");
            }

            var viewModel = new ClientDocumentViewModel
            {
                SubmissionDate = DateTime.Now,
                ClientId = client.Id
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(List<IFormFile> files, ClientDocuments viewModel)
        {
            var loggedInclient = await _userManager.GetUserAsync(User);
            if (loggedInclient == null)
            {
                return NotFound("No logged in User");
            }

            var storageClient = _storageClient;
            string bucketName = _bucketName;
            string folderPath = $"client_files/{loggedInclient.Id}";
            var model = new ClientDocuments
            {
                ClientDocumentId = Guid.NewGuid().ToString(),
                Status = DocumentStatus.Pending,
                ClientFiles = new List<ClientFiles>(),
                SubmissionDate = DateTime.Now,
                ClientId = loggedInclient.Id,
            };

            if (files == null || files.Count == 0)
            {
                return BadRequest("No files received. Please try again.");
            }

            if (files != null && files.Count > 0)
            {
                foreach (IFormFile file in files)
                {
                    if (file.Length > 0)
                    {
                        string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                        string objectName = folderPath + uniqueFileName;

                        using var fileStream = file.OpenReadStream();
                        await _storageClient.UploadObjectAsync(_bucketName, objectName, file.ContentType, fileStream);

                        string fileUrl = $"https://storage.googleapis.com/{bucketName}/{objectName}";

                        if (!string.IsNullOrEmpty(fileUrl))
                        {
                            model.ClientFiles.Add(new ClientFiles
                            {
                                SubmissionDate = DateTime.Now,
                                FileUrl = fileUrl
                            });

                        }
                    }
                }
            }
            _context.ClientDocuments.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Home");


            //if (viewModel.ClientFiles != null && viewModel.ClientFiles.Count > 0)
            //{
            //    foreach(IFormFile docs in model.ClientFiles)
            //    {
            //        if(docs.Length>0)
            //        {
            //        }
            //    }
            //}

        }
    }
}
