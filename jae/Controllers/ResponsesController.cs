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

namespace jae.Controllers
{
    public class ResponsesController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ResponsesController(ApplicationDbContext applicationDbContext, IWebHostEnvironment hostingEnvironment)
        {
            _applicationDbContext = applicationDbContext;
            _hostingEnvironment = hostingEnvironment;
        }
        //admin log in ito
        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View();
        }

        // Add this action for handling admin login
        [HttpPost]
        public async Task<IActionResult> AdminLogin(string username, string password)
        {
            // Implement admin authentication logic here
            if (username == "admin" && password == "password123")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Responses"); // Redirect to the Responses controller's Index action
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }
        }

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
        public async Task<IActionResult> Add(addIntern addIntern)
        {
            if (ModelState.IsValid)
            {
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
                    Resume = addIntern.Resume,
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


