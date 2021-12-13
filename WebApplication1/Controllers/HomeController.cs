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
using WebApplication1.ViewModels;

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

        public List<DateTime> getDatesFromDays(string[] WeekDayNames, DateTime Start, DateTime End)
        {
            DateTime startDate = new DateTime(Start.Year, Start.Month, Start.Day);
            DateTime endDate = new DateTime(End.Year, End.Month, End.Day);
            List<DateTime> list = new List<DateTime>();
            while (startDate <= endDate)
            {
                if (WeekDayNames.Any(r => r == startDate.DayOfWeek.ToString()))
                {
                    list.Add(startDate);
                }
                startDate = startDate.AddDays(1);
            }
            return list;
        }

        public ActionResult ViewMarksPart(string Subject, string Quarters)
        {
            User user = db.Users.Where(i => i.Id == getCurrentUserId()).First();

            var daysOfSubjects = (from t in db.Timetable
                                  join s in db.SchoolClasses on t.SchoolClassesId equals s.Id
                                  join sm in db.SchoolMagazine on t.SchoolClassesId equals sm.SchoolClassesId
                                  join a in db.Class on s.ClassId equals a.Id
                                  join e in db.Pupils on a.Id equals e.ClassId
                                  join sub in db.Subject on sm.SubjectId equals sub.Id
                                  where e.UserId == getCurrentUserId() && sub.Name == Subject &&t.SubjectId==sm.SubjectId
                                  select t.DayOfWeek).Distinct().ToList();

            var sch = (from s in db.SchoolClasses
                       join c in db.Class on s.ClassId equals c.Id
                       where s.SchoolId == user.SchoolId && c.ClassNumber == 1 && c.ClassLetter == "Б"
                       select s).First();

            var subb = db.Subject.Where(s => s.Name == Subject).First();
            var marks = (from sm in db.SchoolMagazine
                         join ap in db.AcademicPerformance on sm.Id equals ap.SchoolMagazineId
                         join s in db.Subject on sm.SubjectId equals s.Id
                         where sm.SchoolClassesId == sch.Id && s.Id == subb.Id
                         orderby ap.PupilId, sm.Date
                         select new SchoolMagazineView
                         {
                             Mark = ap.Mark
                         }).ToList();
            List<string> str = new List<string>();
            foreach (var m in marks)
            {
                str.Add(m.Mark);
            }

            var pupils = (from t in db.Timetable
                          join s in db.SchoolClasses on t.SchoolClassesId equals s.Id
                          join c in db.Class on s.ClassId equals c.Id
                          join p in db.Pupils on c.Id equals p.ClassId
                          where c.ClassNumber == 1 && c.ClassLetter == "Б" && p.RegistrateSchoolId == user.SchoolId && p.UserId==getCurrentUserId()
                          orderby p.LastName
                          select p.LastName + " " + p.Name + " " + p.SecondName).Distinct().ToList();



            string[] days = new string[daysOfSubjects.Count()];
            for (int i = 0; i < daysOfSubjects.Count(); i++)
            {
                switch (daysOfSubjects.ElementAt(i).ToLower())
                {
                    case "понедельник":
                        {
                            days[i] = "Monday";
                            break;
                        }
                    case "вторник":
                        {
                            days[i] = "Tuesday";
                            break;
                        }
                    case "среда":
                        {
                            days[i] = "Wednesday";
                            break;
                        }
                    case "четверг":
                        {
                            days[i] = "Thursday";
                            break;
                        }
                    case "пятница":
                        {
                            days[i] = "Friday";
                            break;
                        }
                }
            }
            var quart = db.Quarter.Where(q => q.QuarterName == Quarters).First();
            DateTime start = quart.Start;
            DateTime end = quart.End;
            List<DateTime> dates = getDatesFromDays(days, start, end);
            ViewBag.names = pupils;
            ViewBag.days = dates;

            SchoolMagazineViewModel model = new SchoolMagazineViewModel();
            model.Date = new List<DateTime>();
            model.Pupils_marks = new List<List<string>>();

            model.Subject = Subject;

            List<List<string>> lists = new List<List<string>>();
            for (int i = 0; i < str.Count; i += dates.Count + 1)
            {
                lists.Add(str.GetRange(i, dates.Count + 1));
            }
            double su = 0;
            int countOfMarksInQuarter = 0;
            for (int i = 0; i < lists.Count(); i++)
            {
                for (int j = 0; j < dates.Count() + 1; j += dates.Count())
                {
                    if (j != dates.Count)
                    {
                        model.Pupils_marks.Add(lists[i].GetRange(j, dates.Count()));
                        foreach (var n in lists[i])
                        {
                            if (n != "н")
                            {
                                su += Convert.ToDouble(n);
                            }
                        }
                    }
                    else
                    {

                        for (int k = 0; k < lists[i].Count; k++)
                        {
                            if (lists[i][k] != null && lists[i][k] != "н")
                            {
                                countOfMarksInQuarter++;
                            }
                        }
                        model.Pupils_marks[i].Add(Convert.ToString(Math.Round(su / countOfMarksInQuarter, MidpointRounding.AwayFromZero)));
                        su = 0;
                    }
                    countOfMarksInQuarter = 0;
                }
            }
            ViewBag.Subject = Subject;
            ViewBag.Quart = Quarters;
            return PartialView(model);
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

                    ViewBag.Quarters = db.Quarter.Select(q => q.QuarterName).ToList();
                    var subjects = (from sm in db.SchoolMagazine
                                    join s in db.SchoolClasses on sm.SchoolClassesId equals s.Id
                                    join c in db.Class on s.ClassId equals c.Id
                                    join a in db.AcademicPerformance on sm.Id equals a.SchoolMagazineId
                                    join e in db.Pupils on a.PupilId equals e.Id
                                    join sub in db.Subject on sm.SubjectId equals sub.Id
                                    where e.UserId == getCurrentUserId()
                                    select sub.Name).Distinct().ToList();
                    ViewBag.subjects = subjects;
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
