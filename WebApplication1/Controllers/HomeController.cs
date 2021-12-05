using LinqToExcel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationContext db;
        IWebHostEnvironment _appEnvironment;
        public HomeController(ApplicationContext context,IWebHostEnvironment appEnvironment)
        {
            db = context;
            _appEnvironment = appEnvironment;
        }
        public string getCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                User user = db.Users.Find(getCurrentUserId());
                ViewBag.SchoolId = user.SchoolId;

                var schDir = from r in db.RegistratedSchools
                             join u in db.Users on r.Id equals u.SchoolId
                             join e in db.Employee on u.Id equals e.UserId
                             where e.PositionId == 1
                             select new SchoolDirector
                             {
                                 SchoolName = r.Name,
                                 LastName = e.LastName,
                                 Name = e.Name,
                                 SecondName = e.SecondName,
                                 ImageUrl = e.ImageUrl,
                             };
                List<SchoolDirector> schoolDirectors = new List<SchoolDirector>();
                foreach (var s in schDir)
                {
                    schoolDirectors.Add(s);
                }
                ViewBag.schoolDirectors = schoolDirectors;

                var employeees = from r in db.RegistratedSchools
                                 join u in db.Users on r.Id equals u.SchoolId
                                 join e in db.Employee on u.Id equals e.UserId
                                 join p in db.Position on e.PositionId equals p.Id
                                 where e.RegistrateSchoolId == user.SchoolId
                                 select new EmployeesOfSchool
                                 {
                                     SchoolName = r.Name,
                                     LastName = e.LastName,
                                     Name = e.Name,
                                     SecondName = e.SecondName,
                                     ImageUrl = e.ImageUrl,
                                     UserId = e.UserId,
                                     Position = p.Name
                                 };
                List<EmployeesOfSchool> employeesOfSchool = new List<EmployeesOfSchool>();
                foreach (var s in employeees)
                {
                    employeesOfSchool.Add(s);
                }
                ViewBag.emplyeesOfSchool = employeesOfSchool;

                var classeshref = (from t in db.Timetable
                                  join s in db.SchoolClasses on t.SchoolClassesId equals s.Id
                                  join c in db.Class on s.ClassId equals c.Id
                                  join e in db.Employee on t.TeacherId equals e.Id
                                  where e.UserId == user.Id
                                  select c).ToList();
                ViewBag.classeshref = classeshref;
            }
            return View();
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
