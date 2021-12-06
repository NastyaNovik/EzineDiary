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
                                  where e.UserId == getCurrentUserId()
                                  select c).Distinct().ToList();
                ViewBag.classeshref = classeshref;

                var daysOfSubjects = (from t in db.Timetable
                                      join s in db.SchoolClasses on t.SchoolClassesId equals s.Id
                                      join c in db.Class on s.ClassId equals c.Id
                                      join e in db.Employee on t.TeacherId equals e.Id
                                      join sub in db.Subject on t.SubjectId equals sub.Id
                                      where e.UserId == getCurrentUserId()
                                      select t.DayOfWeek).Distinct().ToList();
                ViewBag.daysOfSubjects = daysOfSubjects;

                if (User.IsInRole("child"))
                {
                    var pppp = (from k in db.Pupils
                                join u in db.Users on k.UserId equals u.Id
                                where u.Id == getCurrentUserId()
                                select k.ClassId).First();
                    var sch = (from s in db.SchoolClasses
                               join c in db.Class on s.ClassId equals c.Id
                               join pp in db.Pupils on c.Id equals pp.ClassId
                               where s.SchoolId == user.SchoolId && s.ClassId == pppp
                               select s).First();


                    var mag = from sm in db.SchoolMagazine
                              join s in db.Subject on sm.SubjectId equals s.Id
                              where sm.SchoolClassesId == sch.Id && sm.Homework != null
                              select new SchoolMagazineView
                              {
                                  Homework = sm.Homework,
                                  Subject=s.Name,
                                  Date = sm.Date.ToLongDateString()
                              };
                   
                    ViewBag.homework = mag;
                }
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
