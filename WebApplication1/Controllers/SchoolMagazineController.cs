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
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace WebApplication1.Controllers
{
    public class SchoolMagazineController : Controller
    {
        private ApplicationContext db;
        public SchoolMagazineController(ApplicationContext context)
        {
            db = context;
        }
        public string getCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        public IActionResult SchoolMagazine(int Class, string ClassLetter, List<string> daysOfSubjects, DateTime Start, DateTime End, string Q)
        {
            User user = db.Users.Where(i => i.Id == getCurrentUserId()).First();
            ViewBag.Q = Q;
            ViewBag.ClassNumber = Class;
            ViewBag.ClassLetter = ClassLetter;
            ViewBag.Quarters = db.Quarter.Select(q => q.QuarterName).ToList();
            var subjects = (from t in db.Timetable
                            join s in db.SchoolClasses on t.SchoolClassesId equals s.Id
                            join c in db.Class on s.ClassId equals c.Id
                            join e in db.Employee on t.TeacherId equals e.Id
                            join sub in db.Subject on t.SubjectId equals sub.Id
                            where e.UserId == getCurrentUserId()
                            select sub.Name).Distinct().ToList();
            ViewBag.subjects = subjects;

            var pupils = (from t in db.Timetable
                          join s in db.SchoolClasses on t.SchoolClassesId equals s.Id
                          join c in db.Class on s.ClassId equals c.Id
                          join p in db.Pupils on c.Id equals p.ClassId
                          where c.ClassNumber == Class && c.ClassLetter == ClassLetter && p.RegistrateSchoolId == user.SchoolId
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
            List<DateTime> dates = getDatesFromDays(days, Start, End);
            ViewBag.names = pupils;
            ViewBag.days = dates;

         
            return View();
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

        public IActionResult FilterBySubject(int Class, string ClassLetter, string Subject, string Quarters)
        {
            var daysOfSubjects = (from t in db.Timetable
                                  join s in db.SchoolClasses on t.SchoolClassesId equals s.Id
                                  join c in db.Class on s.ClassId equals c.Id
                                  join e in db.Employee on t.TeacherId equals e.Id
                                  join sub in db.Subject on t.SubjectId equals sub.Id
                                  where e.UserId == getCurrentUserId() && sub.Name == Subject
                                  select t.DayOfWeek).Distinct().ToList();
            var quart = db.Quarter.Where(q => q.QuarterName == Quarters).First();
            DateTime start = quart.Start;
            DateTime end = quart.End;
            return RedirectToAction("SchoolMagazine", new { Class = Class, ClassLetter = ClassLetter, daysOfSubjects = daysOfSubjects, Start = start, End = end, Q = quart.QuarterName });
        }

        public async Task<IActionResult> ViewMagazine(string Subject, int Class, string ClassLetter, string Quarters)
        {
            User user = db.Users.Where(i => i.Id == getCurrentUserId()).First();



            var daysOfSubjects = (from t in db.Timetable
                                  join s in db.SchoolClasses on t.SchoolClassesId equals s.Id
                                  join c in db.Class on s.ClassId equals c.Id
                                  join e in db.Employee on t.TeacherId equals e.Id
                                  join sub in db.Subject on t.SubjectId equals sub.Id
                                  where e.UserId == getCurrentUserId() && sub.Name == Subject
                                  select t.DayOfWeek).Distinct().ToList();
            
            var sch = (from s in db.SchoolClasses
                       join c in db.Class on s.ClassId equals c.Id
                       where s.SchoolId == user.SchoolId && c.ClassNumber == Class && c.ClassLetter == ClassLetter
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
                          where c.ClassNumber == Class && c.ClassLetter == ClassLetter && p.RegistrateSchoolId == user.SchoolId
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
            model.Class = Class;
            model.ClassLetter = ClassLetter;

            List<List<string>> lists = new List<List<string>>();
            for (int i = 0; i < str.Count; i += dates.Count+1)
            {
                lists.Add(str.GetRange(i, dates.Count+1));
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
                            su = lists[i].Sum(x => Convert.ToDouble(x));
                        }
                        else
                        {

                            for (int k = 0; k < lists[i].Count; k++)
                            {
                                if (lists[i][k] != null)
                                {
                                    countOfMarksInQuarter++;
                                }
                            }
                            model.Pupils_marks[i].Add(Convert.ToString(Math.Round(su / countOfMarksInQuarter, MidpointRounding.AwayFromZero)));
                        }                    
                    countOfMarksInQuarter = 0;
                }
            }
            ViewBag.clas = Class;
            ViewBag.ClassLetter = ClassLetter;
            ViewBag.Subject = Subject;
            ViewBag.Quart = Quarters;
            return View("ViewMagazine", model);
        }

        public async Task<IActionResult> SaveMagazine(SchoolMagazineViewModel model, int Class, string ClassLetter, string Quarters)
        {
            Subject subject = db.Subject.Where(s => s.Name == model.Subject).First();
            Employee emp = db.Employee.Where(e => e.UserId == getCurrentUserId()).First();
            var sch = (from s in db.SchoolClasses
                       join c in db.Class on s.ClassId equals c.Id
                       where s.SchoolId == emp.RegistrateSchoolId && c.ClassNumber == Class && c.ClassLetter == ClassLetter
                       select s.Id).ToList();

            Class cl = db.Class.Where(c => c.ClassNumber == Class && c.ClassLetter == ClassLetter).First();

            List<DateTime> dates = model.Date;

            var pupils = (from t in db.Timetable
                          join s in db.SchoolClasses on t.SchoolClassesId equals s.Id
                          join c in db.Class on s.ClassId equals c.Id
                          join p in db.Pupils on c.Id equals p.ClassId
                          where c.ClassNumber == Class && c.ClassLetter == ClassLetter && p.RegistrateSchoolId == emp.RegistrateSchoolId
                          orderby p.LastName
                          select p.LastName + " " + p.Name + " " + p.SecondName).Distinct().ToList();

            List<List<string>> pupils_marks = new List<List<string>>();
            for (int i=0;i<pupils.Count;i++)
            {
                for (int j = 0; j < model.Pupils_marks[i].Count; j += dates.Count + 2)
                {
                    pupils_marks.Add(model.Pupils_marks[i].GetRange(j, dates.Count + 2));
                }
            }

            List<SchoolMagazine> schmg = new List<SchoolMagazine>();

            foreach (var d in dates)
            {
                SchoolMagazine magazine = new SchoolMagazine()
                {
                    SubjectId = subject.Id,
                    TeacherId = emp.Id,
                    Date = d,
                    SchoolId = emp.RegistrateSchoolId,
                    SchoolClassesId = sch.First()
                };
                schmg.Add(magazine);
            }
            SchoolMagazine magazine2 = new SchoolMagazine
            {
                SubjectId = subject.Id,
                TeacherId = emp.Id,
                Date = new DateTime(3000, 12, 31),
                SchoolId = emp.RegistrateSchoolId,
                SchoolClassesId = sch.First()
            };
            schmg.Add(magazine2);
            int count = schmg.Count();
            await db.SchoolMagazine.AddRangeAsync(schmg);
            await db.SaveChangesAsync();

            var pupIds = db.Pupils.Where(s => s.RegistrateSchoolId == emp.RegistrateSchoolId && s.ClassId == cl.Id).ToList();

            var da = db.SchoolMagazine.OrderByDescending(i => i.Id).Take(count).ToList();
            var dasort = da.OrderBy(s => s.Date).ToList();
            List<AcademicPerformance> acad = new List<AcademicPerformance>();
            for (int i = 0; i < pupils_marks.Count(); i++)
            {
                pupils_marks[i].RemoveAt(0);
                for (int j = 0; j < dates.Count()+1; j++)
                {
                    AcademicPerformance academicPerformance = new AcademicPerformance
                    {
                        PupilId = pupIds[i].Id,
                        Mark = pupils_marks[i][j],
                        SchoolMagazineId = dasort[j].Id,
                    };
                    acad.Add(academicPerformance);

                }
            }
            await db.AcademicPerformance.AddRangeAsync(acad);
            await db.SaveChangesAsync();

            return RedirectToAction("ViewMagazine", new { Subject=subject.Name, Class= Class, ClassLetter= ClassLetter, Quarters= Quarters });
        }

        public IActionResult AddHomework(SchoolMagazineViewModel model, int Class, string ClassLetter, DateTime dates)
        {
            User user = db.Users.Where(i => i.Id == getCurrentUserId()).First();
            var sch = (from s in db.SchoolClasses
                       join c in db.Class on s.ClassId equals c.Id
                       where s.SchoolId == user.SchoolId && c.ClassNumber == Class && c.ClassLetter == ClassLetter
                       select s).First();

            IQueryable<SchoolMagazine> sc = db.SchoolMagazine.Where(s => s.Date == dates && s.SchoolClassesId == sch.Id);
            foreach (var s in sc)
            {
                s.Homework = model.Homework;
            }
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
    }
}
