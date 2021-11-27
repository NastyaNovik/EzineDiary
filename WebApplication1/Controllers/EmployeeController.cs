using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using WebApplication1.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using WebApplication1.ViewModels;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Controllers
{
    public class EmployeeController : Controller
    {
        Account account = new Account("dwigle", "182646829522575", "zh2QL2GT8Gi8R5e8wDIJPwiTTFs");
        private ApplicationContext db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private static string DefaultImageUrl= "https://res.cloudinary.com/dwigle/image/upload/v1629306209/images/uyut_bishkek_a4qq8l.png";
        public EmployeeController(ApplicationContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            db = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string getCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        public string LoadImage(IFormFile uploadedFile)
        {
            Cloudinary cloudinary = new Cloudinary(account);
            if (uploadedFile != null)
            {
                var uploadParams = new ImageUploadParams()
                {
                    Folder = "images",
                    File = new FileDescription(uploadedFile.FileName, uploadedFile.OpenReadStream()),
                };
                var uploadResult = cloudinary.Upload(uploadParams);
                return Convert.ToString(uploadResult.Url);
            }
            else return DefaultImageUrl;
        }
        [HttpGet]
        public IActionResult AddEmployee(string position)
        {
            ViewBag.position = position;
            ViewBag.Subjects = db.Subject.ToList();
            return View();
        }
        [HttpPost]
        public async Task <IActionResult> AddEmployee(IFormFile uploadedFile, string position, AddEmployeeViewModel model, string[] subjects)
        {
            int count = db.Employee.Where(p => p.LastName == model.LastName && p.Name == model.Name && p.SecondName == model.SecondName && p.PhoneNumber == model.PhoneNumber).Count();
            if (count == 0)
            {
                string result = "";
                foreach (string p in subjects)
                {
                    result += p;
                    result += ",";
                }
                User user = await db.Users.OrderByDescending(u => u.SchoolId).FirstAsync();
                User user2 = await db.Users.FindAsync(getCurrentUserId());
                Position ChPosition = db.Position.Where(p => p.Name == position).First();
                if (User.IsInRole("admin"))
                {
                    Employee employee = new Employee
                    {
                        LastName = model.LastName,
                        Name = model.Name,
                        SecondName = model.SecondName,
                        PhoneNumber = model.PhoneNumber,
                        AdditionalInformation = model.AdditionalInformation,
                        PositionId = ChPosition.Id,
                        RegistrateSchoolId = user.SchoolId,
                        UserId = user.Id,
                        Subject = result,
                        ImageUrl = LoadImage(uploadedFile)
                    };
                    db.Employee.Add(employee);
                }
                else
                {
                    Employee employee = new Employee
                    {
                        LastName = model.LastName,
                        Name = model.Name,
                        SecondName = model.SecondName,
                        PhoneNumber = model.PhoneNumber,
                        AdditionalInformation = model.AdditionalInformation,
                        PositionId = ChPosition.Id,
                        RegistrateSchoolId = user2.SchoolId,
                        UserId = user.Id,
                        Subject = result,
                        ImageUrl = LoadImage(uploadedFile)
                    };
                    db.Employee.Add(employee);
                }
                
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Этот сотрудник уже существует!");
            }
            return RedirectToAction("AddEmployee");
        }
        [HttpGet]
        public IActionResult RegisterTeachersAdministration()
        {
            ViewBag.Positions = db.Position.Where(a=>a.Name!="Директор").ToList();
            if (User.Identity.IsAuthenticated)
            {
                User user = db.Users.Find(getCurrentUserId());
                ViewBag.SchoolId = user.SchoolId;
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterTeachersAdministration(RegistrateTeacherAdministration model, int SchoolId)
        {
            User user = new User { Email = model.Email, UserName = model.UserName, SchoolId = SchoolId };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (model.Position == "Учитель")
                {
                    await _userManager.AddToRoleAsync(user, "teacher");
                    return RedirectToAction("AddEmployee", "Employee", new { position = model.Position });
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "administration");
                    return RedirectToAction("AddEmployee", "Employee", new { position = model.Position });
                }
            }
            return View();
        }
    }
}
