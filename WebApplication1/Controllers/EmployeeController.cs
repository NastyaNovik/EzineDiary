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
        RoleManager<IdentityRole> _roleManager;
        private static string DefaultImageUrl= "https://res.cloudinary.com/dwigle/image/upload/v1629306209/images/uyut_bishkek_a4qq8l.png";
        public EmployeeController(ApplicationContext context, RoleManager<IdentityRole> roleManager, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            db = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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
                User user = await db.Users.LastAsync();
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
        public async Task<IActionResult> DeleteEmployee(string UserId)
        {
            Employee employee = db.Employee.Where(e => e.UserId == UserId).First();
            User user = await _userManager.FindByIdAsync(UserId);
            if (user != null)
            {
                IEnumerable<string> userRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, userRoles);
                await _userManager.DeleteAsync(user);
                db.Employee.Remove(employee);
            }
            await db.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> EditEmployee(string UserId, int SchoolId, string Error)
        {
            ViewBag.Error = Error;
            ViewBag.SchoolId = SchoolId;
            User user = await _userManager.FindByIdAsync(UserId);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                allRoles.Remove(db.Roles.Where(u => u.Name == "Child").First());
                allRoles.Remove(db.Roles.Where(u => u.Name == "admin").First());
                ChangeRoleViewModel model = new ChangeRoleViewModel()
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRoles = userRoles,
                    AllRoles = allRoles
                };
                return View("EditEmployee", model);
            }
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> EditEmployee(string UserId, List<string> roles, int SchoolId)
        {
           
            User user = await _userManager.FindByIdAsync(UserId);
            if (user != null)
            {
                if (roles.Contains("teacher"))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var addedRoles = roles.Except(userRoles);
                    var removedRoles = userRoles.Except(roles);
                    await _userManager.AddToRolesAsync(user, addedRoles);
                    await _userManager.RemoveFromRolesAsync(user, removedRoles);
                    Employee employee = db.Employee.Where(e => e.UserId == UserId).First();
                    Position position = db.Position.Where(p => p.Name == "Учитель").First();
                    employee.PositionId = position.Id;
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                else if (roles.Contains("administration"))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var addedRoles = roles.Except(userRoles);
                    var removedRoles = userRoles.Except(roles);
                    await _userManager.AddToRolesAsync(user, addedRoles);
                    await _userManager.RemoveFromRolesAsync(user, removedRoles);
                    Employee employee = db.Employee.Where(e => e.UserId == UserId).First();
                    Position position = db.Position.Where(p => p.Name == "Заместитель директора по учебной части").First();
                    employee.PositionId = position.Id;
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                else if(roles.Contains("Директор"))
                {
                    int countDir = db.Employee.Where(e => e.PositionId==1&&e.RegistrateSchoolId==SchoolId).Count();
                    if (countDir == 0)
                    {
                        var userRoles = await _userManager.GetRolesAsync(user);
                        var removedRoles = userRoles.Except(roles);
                        await _userManager.RemoveFromRolesAsync(user, removedRoles);
                        await _userManager.AddToRoleAsync(user, "administration");
                        Employee employee = db.Employee.Where(e => e.UserId == UserId).First();
                        Position position = db.Position.Where(p => p.Name == "Директор").First();
                        employee.PositionId = position.Id;
                        db.SaveChanges();
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("EditEmployee","Employee",new { UserId = user.Id, SchoolId = SchoolId, Error="В этой школе уже есть директор!" });
                    }
                }
                else if (roles.Contains("Заместитель директора по учебной части"))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var removedRoles = userRoles.Except(roles);
                    await _userManager.RemoveFromRolesAsync(user, removedRoles);
                    await _userManager.AddToRoleAsync(user, "administration");
                    Employee employee = db.Employee.Where(e => e.UserId == UserId).First();
                    Position position = db.Position.Where(p => p.Name == "Заместитель директора по учебной части").First();
                    employee.PositionId = position.Id;
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                else if (roles.Contains("Заместитель директора по воспитательной работе"))
                {
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var removedRoles = userRoles.Except(roles);
                    await _userManager.RemoveFromRolesAsync(user, removedRoles);
                    await _userManager.AddToRoleAsync(user, "administration");
                    Employee employee = db.Employee.Where(e => e.UserId == UserId).First();
                    Position position = db.Position.Where(p => p.Name == "Заместитель директора по воспитательной работе").First();
                    employee.PositionId = position.Id;
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
            }
            return NotFound();
        }
    }
}
