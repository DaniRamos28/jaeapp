using jae.Data;
using OfficeOpenXml;
using System.IO;
using System.Web;
using jae.Models;
using jae.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcelDataReader;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using jae.Areas.Identity.Data;
using jae.Areas.Identity.Pages.Account;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc.ViewFeatures;


namespace jae.Controllers
{
    public class ResponsesController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<jaeUser> _userManager;

        public ResponsesController(ApplicationDbContext applicationDbContext, IWebHostEnvironment hostingEnvironment, UserManager<jaeUser> userManager)
        {
            _applicationDbContext = applicationDbContext;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
        }


        // Add this attribute to restrict access to authenticated users
        [Authorize]

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var responses = await _applicationDbContext.Responses.ToListAsync();
            return View(responses);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GetList()
        {
            var responses = _applicationDbContext.Responses.ToList();
            return Json(new { data = responses });
        }
        [HttpPost]
        public IActionResult SendEmail(string recipient, string subject, string message)
        {
            try
            {
                string fromMail = "dnramos011@gmail.com";
                string fromPassword = "gcagzjnyexpkbuki";
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromMail, fromPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("dnramos011@gmail.com"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(recipient);

                smtpClient.Send(mailMessage);
                return Json(new { result = "success" });

            }
            catch (Exception ex)
            {
                // Handle and log the error
                return Json(new { result = "error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(addIntern addIntern)
        {
            if (ModelState.IsValid)
            {
                string? resumeFileName = null;
                if (addIntern.Resume != null && addIntern.Resume.Length > 0)
                {
                    // Generate a unique file name (you can customize the file naming strategy)
                    resumeFileName = Guid.NewGuid().ToString() + Path.GetExtension(addIntern.Resume.FileName);

                    // Define the directory where the resumes will be saved (adjust this path as needed)
                    string resumeDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "resumes");

                    // Combine the directory and file name to create the full path
                    string resumeFilePath = Path.Combine(resumeDirectory, resumeFileName);

                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(resumeDirectory);

                    // Save the uploaded resume to the specified file path
                    using (var fileStream = new FileStream(resumeFilePath, FileMode.Create))
                    {
                        await addIntern.Resume.CopyToAsync(fileStream);
                    }
                }

                var intern = new Responses()
                {
                    Response = DateTime.Now,
                    Name = addIntern.Name,
                    Course = addIntern.Course,
                    School = addIntern.School,
                    Number = addIntern.Number,
                    wfhft = addIntern.wfhft,
                    Datestart = addIntern.Datestart,
                    Renderhrs = addIntern.Renderhrs,
                    Email = addIntern.Email,
                    Address = addIntern.Address,
                    Resume = resumeFileName,
                    Status = "Pending"
                };

                try
                {
                    _applicationDbContext.Responses.Add(intern);
                    await _applicationDbContext.SaveChangesAsync();
                    return RedirectToAction("Add");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while adding the response: " + ex.Message);
                }

            }

            return View(addIntern);
        }


        [HttpPost]
        public async Task<IActionResult> Index(IFormFile excelfile, [FromServices] IWebHostEnvironment hostingEnvironment)
        {
            if (excelfile != null && excelfile.Length > 0)
            {
                string fileName = Path.Combine(hostingEnvironment.WebRootPath, "files", excelfile.FileName);

                try
                {
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                    {
                        await excelfile.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                    }

                    var responses = await GetResponsesListAsync(excelfile.FileName);
                    return View(responses);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while processing the file: " + ex.Message);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Please select a file to upload.");
            }

           
            return View();
        }

        public IActionResult DownloadResume(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                var file = Path.Combine(_hostingEnvironment.WebRootPath, "resumes", filename);

                if (System.IO.File.Exists(file))
                {
                    return File(System.IO.File.OpenRead(file), "application/octet-stream", filename);
                }
            }

            return NotFound();
        }


        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var responses = await _applicationDbContext.Responses.FirstOrDefaultAsync(x => x.Id == id);

            if (responses != null)
            {
                var viewModel = new EditResponse()
                {
                    Id = responses.Id,
                    Name = responses.Name,
                    Course = responses.Course,
                    School = responses.School,
                    Number = responses.Number,
                    wfhft = responses.wfhft,
                    Datestart = responses.Datestart,
                    Renderhrs = responses.Renderhrs,
                    Email = responses.Email,
                    Address = responses.Address,
                    Resume = responses.Resume,
                    Status = responses.Status
                };

                return View("View", viewModel);
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> View(EditResponse model)
        {
            if (ModelState.IsValid)
            {
                var responses = await _applicationDbContext.Responses.FindAsync(model.Id);
                if (responses != null)
                {
                    responses.Id = model.Id;
                    responses.Name = model.Name;
                    responses.Course = model.Course;
                    responses.School = model.School;
                    responses.Number = model.Number;
                    responses.wfhft = model.wfhft;
                    responses.Datestart = model.Datestart;
                    responses.Renderhrs = model.Renderhrs;
                    responses.Email = model.Email;
                    responses.Address = model.Address;
                    responses.Resume = model.Resume;
                    responses.Status = model.Status;

                    try
                    {
                        await _applicationDbContext.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, "An error occurred while updating the response: " + ex.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Response not found.");
                }
            }


            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Delete(EditResponse model)
        {
            try
            {
                var response = await _applicationDbContext.Responses.FindAsync(model.Id);
                if (response != null)
                {
                    _applicationDbContext.Responses.Remove(response);
                    await _applicationDbContext.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the response: " + ex.Message);
            }

            return RedirectToAction("Index");
        }



        private async Task<List<Responses>> GetResponsesListAsync(string fName)
        {
            List<Responses> responses = new List<Responses>();
            var fileName = Path.Combine(_hostingEnvironment.WebRootPath, "files", fName);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        var response = new Responses()
                        {
                            Response = DateTime.ParseExact(reader.GetDateTime(0).ToString("M/dd/yyyy HH:mm:ss"), "M/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                            Name = reader.GetString(1),
                            Course = reader.GetString(2),
                            School = reader.GetString(3),
                            Number = reader.GetString(4),
                            wfhft = reader.GetString(5),
                            Datestart = DateTime.ParseExact(reader.GetDateTime(6).ToString("M/dd/yyyy HH:mm:ss"), "M/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).Date,
                            Renderhrs = reader.GetString(7),
                            Email = reader.GetString(8),
                            Address = reader.GetString(9),
                            Resume = reader.GetString(10), 
                        };

                        responses.Add(response);
                    }
                }
            }

            try
            {
                _applicationDbContext.Responses.AddRange(responses);
                await _applicationDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while importing the file: " + ex.Message);
            }

             return responses;

        }
    }

}


