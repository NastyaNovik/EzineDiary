using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.ViewModels;
using ClosedXML.Excel;
using WebApplication1.Models;
using System.IO;
using ExcelDataReader;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    public class PupilController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private ApplicationContext db;
        private IWebHostEnvironment _environment;

        public PupilController(ApplicationContext context, IWebHostEnvironment env, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            db = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = env;
        }
        public string getCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        [HttpGet]
        public IActionResult AddPupils()
        {
            User user = db.Users.Find(getCurrentUserId());
            ViewBag.SchoolId = user.SchoolId;
            ViewBag.SchoolClasses = db.Class.Select(c=>c.ClassNumber).Distinct().ToList();
            ViewBag.SchoolLetters = db.Class.Select(c => c.ClassLetter).Distinct().ToList();
            ViewBag.Teachers = db.Employee.Where(e => e.RegistrateSchoolId == user.SchoolId && e.PositionId == 4);
            return View();
        }
        public string getUserIdFromUserName(string UserName)
        {
            User user = db.Users.Where(u => u.UserName == UserName).First();
            return user.Id;
        }
        [HttpPost]
        public async Task<IActionResult> AddPupils(IFormFile fileExcel, int Classes, string Letters, int TeacherId)
        {
            User user1 = db.Users.Find(getCurrentUserId());
            List<PupilsViewModel> pupilsRegistr = new List<PupilsViewModel>();
            string fName = fileExcel.FileName;
            string path = Path.Combine(_environment.ContentRootPath, "uploads/" + fName);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var stream = new MemoryStream())
            {
                await fileExcel.CopyToAsync(stream);
                stream.Position = 0;
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                        while (reader.Read())
                        {
                            pupilsRegistr.Add(new PupilsViewModel
                            {
                                UserName = reader.GetValue(0).ToString(),
                                Email = reader.GetValue(0).ToString() + "@schools.by",
                                Password = reader.GetValue(0).ToString(),
                                SchoolId = user1.SchoolId
                            });
                        }
                }
            }
            foreach (var pup in pupilsRegistr)
            {
                User user = new User { Email = pup.Email, UserName = pup.UserName, SchoolId = pup.SchoolId };
                var result = await _userManager.CreateAsync(user, pup.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "child");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            Class clas = db.Class.Where(c => c.ClassNumber == Classes && c.ClassLetter == Letters).First();
            List<PupilsViewModel> pupils = new List<PupilsViewModel>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (var stream = new MemoryStream())
            {
                fileExcel.CopyTo(stream);
                stream.Position = 0;
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        pupils.Add(new PupilsViewModel
                        {
                            LastName = reader.GetValue(1).ToString(),
                            Name = reader.GetValue(2).ToString(),
                            SecondName = reader.GetValue(3).ToString(),
                            PhoneNumber = reader.GetValue(4).ToString(),
                            SchoolId = user1.SchoolId,
                            ClassNumber = clas.ClassNumber,
                            ClassLetter = clas.ClassLetter,
                            UserId = getUserIdFromUserName(reader.GetValue(0).ToString())
                        });
                    }
                }
            }
            Pupil pupil = new Pupil();
            foreach (var p in pupils)
            {
                pupil.LastName = p.LastName;
                pupil.Name = p.Name;
                pupil.SecondName = p.SecondName;
                pupil.PhoneNumber = p.PhoneNumber;
                pupil.RegistrateSchoolId = p.SchoolId;
                pupil.ClassId = clas.Id;
                pupil.UserId = p.UserId;
                db.Pupils.Add(pupil);
                db.SaveChanges();
                pupil.Id = 0;
            }
            SchoolClasses schCl = new SchoolClasses();
            schCl.SchoolId = user1.SchoolId;
            schCl.ClassId = clas.Id;
            schCl.ClassroomTeacherId = TeacherId;
            if (db.SchoolClasses.Where(a => a.SchoolId == schCl.SchoolId && a.ClassId == schCl.ClassId).Count() == 0)
            {
                db.SchoolClasses.Add(schCl);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index","Home");
        }
    }
}

