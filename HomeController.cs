using Google.Cloud.Storage.V1;
using System;
using System.IO;
using System.Threading.Tasks;
using L.R._Gcaleka__Co.Data;
using L.R._Gcaleka__Co.Models;
using L.R._Gcaleka__Co.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace L.R._Gcaleka__Co.Controllers
{
    [Authorize(Roles = "Admin, Candidate Attorney")]
    public class HomeController : Controller
    {
        private readonly GcsSignedUrlService _gcsSignedUrlService;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CloudStorageService _storageService;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, GcsSignedUrlService gcsSignedUrlService)
        {
            _gcsSignedUrlService = gcsSignedUrlService;
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _storageService = new CloudStorageService(); //initializing cloudstorage
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AdminHome()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var diarisedfiles = await _context.Diarise
                .Where(d => d.ScheduledDate == today && d.IsCompleted == false)
                .Include(d => d.File)
                .ToListAsync();

            return View(diarisedfiles);
        }
        public async Task<IActionResult> UnMarkedFiles()
        {
            var grouped = await _context.Diarise
                .Include(d => d.File)
                .Where(d => d.IsUnreviewed && !d.IsCompleted)
                .GroupBy(d => d.ScheduledDate.Date)
                .Select(d => new DiariseUnreviewedFiles
                {
                    DateScheduled = d.Key,
                    Files = d.ToList()
                })
                .OrderBy(d => d.DateScheduled)
                .ToListAsync();

            return View(grouped);
          //var today = DateTime.Today;
            //var unreviewedFiles = await _context.Diarise
            //    .Include(d => d.File)
            //    .Where(d => d.IsUnreviewed == true && d.IsCompleted == false)
            //    .GroupBy(d=>d.ScheduledDate) //I just added this line
            //    .OrderBy(d=>d.Key) //and this one as well
            //    .ToListAsync();

            //return View(unreviewedFiles);
        }

        [HttpGet]
        public async Task<IActionResult> CreateFile(string id)
        {
            var client = await _context.Clientele.FindAsync(id);
            if (client == null)
            {
                return NotFound("Oops we couldn't find the client you're looking for! you might want to add them first.");
            }

            var employee = await _userManager.GetUserAsync(User);
            if (employee == null)
            {
                return Unauthorized("User not logged in.");
            }
            ViewBag.CaseTypes = new List<SelectListItem>
            {
                   new SelectListItem { Value = "Criminal", Text = "Criminal" },
                   new SelectListItem { Value = "Civil", Text = "Civil" },
                   new SelectListItem { Value = "Family", Text = "Family" },
                   new SelectListItem { Value = "Labor", Text = "Labor" }
            };

            var viewModel = new FileViewModel
            {
                ClienteleId = client.ClienteleId,
                OpeningDate = DateTime.Now,
                EmployeeId = employee.Id

            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFile(FileViewModel viewModel)
        {
            _logger.LogInformation("Received File Creation Request.");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid");
                _logger.LogInformation("ModelState is invalid");

                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }

            var employee = await _userManager.GetUserAsync(User);
            var client = await _context.Clientele.FindAsync(viewModel.ClienteleId);
            if (client == null)
            {
                _logger.LogError("Client not found.");
                return NotFound("Client not found.");
            }


            ViewBag.CaseTypes = new List<SelectListItem>
            {
                   new SelectListItem { Value = "Criminal", Text = "Criminal" },
                   new SelectListItem { Value = "Civil", Text = "Civil" },
                   new SelectListItem { Value = "Family", Text = "Family" },
                   new SelectListItem { Value = "Labor", Text = "Labor" }
            };

            _logger.LogInformation("Creating new file entry.");
            var file = new Files
            {
                FileId = Guid.NewGuid().ToString(),
                FileName = viewModel.FileName,
                ClienteleId = viewModel.ClienteleId,
                OpeningDate = viewModel.OpeningDate,
                CaseType = viewModel.CaseType,
                CaseStatus = viewModel.CaseStatus,
                EmployeeId = employee.Id,
                Documents = new List<Document>()
            };

            if (file.Documents == null)
            {
                file.Documents = new List<Document>(); // nullcheck Prevents NullReferenceException
            }

            if (viewModel.Documents != null && viewModel.Documents.Count > 0)
            {
                _logger.LogInformation($"Total Documents Received: {viewModel.Documents.Count}");
                foreach (IFormFile doc in viewModel.Documents)
                {
                    if (doc.Length > 0)
                    {
                        string fileUrl = await _storageService.UploadFileAsync(doc);
                        if (!string.IsNullOrEmpty(fileUrl))
                        {
                            file.Documents.Add(new Document
                            {
                                FileUrl = fileUrl,
                                UploadedAt = DateTime.Now,
                            });
                        }
                    }
                }
            }
            if (file == null)
            {
                Console.WriteLine("Error:File Object is null before adding to database");
                return View(viewModel);
            }
            _context.Files.Add(file);
            await _context.SaveChangesAsync();

            return RedirectToAction("FileDetails", new { id = file.FileId });
        }

        public async Task<IActionResult> FileDetails(string id)
        {
            var file = await _context.Files
                .Include(f => f.Client)
                .Include(fd => fd.Documents)
                .FirstOrDefaultAsync(f => f.FileId == id);
            if (file == null)
            {
                return NotFound();
            }

            foreach (var doc in file.Documents)
            {
                string objectName = doc.FileUrl.Replace("https://storage.googleapis.com/lrgcaleka-uploads/", "");
                doc.FileUrl = _gcsSignedUrlService.GenerateSignedUrl(objectName, TimeSpan.FromMinutes(15));
            }

            return View(file);
        }

        public IActionResult AllFiles(string searchString)
        {
            var files = _context.Files
                .Include(f => f.Client)
                .ToList();

            if (!String.IsNullOrEmpty(searchString))
            {
                files = files.Where(c => c.FileName.Contains(searchString)).ToList();
            }
            return View(files);
        }

        [HttpGet]
        public async Task<IActionResult> Diarising(string id)
        {
            var files = await _context.Files.FindAsync(id);
            if (files == null)
            {
                return NotFound("We looked the file you selected, unfortunately we couldn't find it.");
            }
            var diarise = new Diarise
            {
                ScheduledDate = DateTime.Now,
                FileId = files.FileId
            };
            return View(diarise);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Diarising(string FileId, DateTime ScheduledDate, bool IsCourtDate, string? CourtName)
        {
            var file = await _context.Files.FindAsync(FileId);
            if (file == null)
            {
                return NotFound("File not found.");
            }

            var diarisingEntry = new Diarise
            {
                FileId = FileId,
                ScheduledDate = ScheduledDate,
                IsCourtDate = IsCourtDate,
                CourtDate = IsCourtDate ? ScheduledDate : null,
                CourtName = IsCourtDate ? CourtName : null,
                IsCompleted = false,
                IsUnreviewed = false,
            };
            _context.Diarise.Add(diarisingEntry);
            await _context.SaveChangesAsync();

            return RedirectToAction("FileDetails", new { id = FileId });
        }


        [HttpGet]
        public async Task<IActionResult> AddClient()
        {
            var employee = await _userManager.GetUserAsync(User);
            ViewBag.ProvinceId = new SelectList(_context.Province, "ProvinceId", "ProvinceName");
            ViewBag.CityId = new SelectList(_context.City, "CityId", "CityName");
            ViewBag.SuburbId = new SelectList(_context.Suburb, "SuburbId", "SuburbName");
            ViewBag.PostalCodeId = new SelectList(_context.PostalCode, "PostalCodeId", "ZipCode");

            var viewModel = new ClienteleViewModel
            {
                EmployeeId = employee.Id,
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClient(ClienteleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProvinceId = new SelectList(_context.Province, "ProvinceId", "ProvinceName");
                ViewBag.CityId = new SelectList(_context.City, "CityId", "CityName");
                ViewBag.SuburbId = new SelectList(_context.Suburb, "SuburbId", "SuburbName");
                ViewBag.PostalCodeId = new SelectList(_context.PostalCode, "PostalCodeId", "ZipCode");

                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.EmailAddress);
            var clientele = new Clientele
            {
                ClienteleId = existingUser != null ? existingUser.Id : Guid.NewGuid().ToString(),
                ClientType = model.ClientType,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IdentityNumber = model.IdentityNumber,
                DateOfBirth = model.DateOfBirth,
                CompanyName = model.CompanyName,
                ContactNumber = model.ContactNumber,
                AltContactNumber = model.AltContactNumber,
                EmailAddress = model.EmailAddress,
                ProvinceId = model.SelectedProvinceId,
                CityId = model.SelectedCityId,
                SuburbId = model.SelectedSuburbId,
                PostalCodeId = model.SelectedPostalCodeId,
                StreetAddress = model.StreetAddress,
                EmployeeId = model.EmployeeId,
                AspNetUserId = existingUser?.Id
            };

            _context.Clientele.Add(clientele);
            await _context.SaveChangesAsync();
            return RedirectToAction("AddClient");

        }
        public IActionResult ClientList(string searchString)
        {
            var clients = _context.Clientele.ToList();

            if (clients == null)
            {
                return NotFound("N/A");
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                clients = clients.Where(c => c.LastName.Contains(searchString)).ToList();
            }
            return View(clients);
        }

        public async Task<IActionResult> ClientDetails(string id)
        {
            var client = await _context.Clientele.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }
        [HttpGet]
        public async Task<IActionResult> Appointments()
        {
            var appointments = _context.Appointment
                .Include(a => a.LoggedInClient)
                .ToList();
            return View(appointments);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseFile(string id, string closeNote)
        {
            var file = await _context.Files.FindAsync(id);
            if(file==null)
            {
                return NotFound();
            }

            file.IsClosed = true;
            file.CaseStatus = CaseStatus.Closed;
            file.CloseDate = DateTime.Now;
            file.CloseNote = closeNote;

            _context.Update(file);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(FileDetails), new {id=file.FileId});
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
