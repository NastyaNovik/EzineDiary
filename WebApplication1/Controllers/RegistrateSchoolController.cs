using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Controllers
{
    public class RegistrateSchoolController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private ApplicationContext db;
        IWebHostEnvironment _appEnvironment;
        public RegistrateSchoolController(ApplicationContext context, IWebHostEnvironment appEnvironment,UserManager<User> userManager, SignInManager<User> signInManager)
        {
            db = context;
            _appEnvironment = appEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public ActionResult RegistrateSchool()
        {
            List<School> schoolList =db.Schools.ToList();
            ViewBag.schools = schoolList;
            return View();
        }

        public async Task <IActionResult> Register(RegistrateSchoolViewModel model)
        {
            int count = db.RegistratedSchools.Where(a => a.Name == model.Name).Count();
            if (count == 0)
            {
                RegisteredSchool school = new RegisteredSchool { Name = model.Name };
                db.RegistratedSchools.Add(school);
                await db.SaveChangesAsync();
                if (ModelState.IsValid)
                {
                    User user = new User { Email = model.Email, UserName = model.UserName, SchoolId = school.Id };
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "administration");
                        return RedirectToAction("AddEmployee", "Employee", new { position = model.Position });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            return RedirectToAction("RegistrateSchool");
        }
    }
}
