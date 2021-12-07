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
            //  User user = db.Users.Where(i => i.Id == getCurrentUserId()).First();

            //var sch = (from s in db.SchoolClasses
            //           join c in db.Class on s.ClassId equals c.Id
            //           where s.SchoolId == user.SchoolId && c.ClassNumber == Class && c.ClassLetter == ClassLetter
            //           select s).First();
            //var mag = (from sm in db.SchoolMagazine
            //           join ap in db.AcademicPerformance on sm.Id equals ap.SchoolMagazineId
            //           where sm.SchoolClassesId == sch.Id
            //           select new SchoolMagazineView
            //           {
            //               SubjectId = sm.SubjectId,
            //               TeacherId = sm.TeacherId,
            //               Date = sm.Date,
            //               SchoolClassesId = sm.SchoolClassesId,
            //               SchoolId = sm.SchoolId,
            //               PupilId = ap.PupilId,
            //               Mark = ap.Mark
            //           }).ToList();
            //SchoolMagazineViewModel model = new SchoolMagazineViewModel();
            //model.Date = new List<DateTime>();
            //model.Pupils_marks = new List<string>();
            //foreach (var m in mag)
            //{
            //    model.Date.Add(m.Date);
            //    model.Pupils_marks.Add(m.Mark);
            //}

            return View(/*model*/);
        }
        public IActionResult EditMagazineAndView(int Class, string ClassLetter, string Subject, DateTime Start, DateTime End, string Q, SchoolMagazineViewModel model, string Quarters)
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
            return RedirectToAction("SchoolMagazine", new { Class = Class, ClassLetter = ClassLetter, daysOfSubjects = daysOfSubjects, Start = start, End = end, Q = quart.QuarterName });
            //return View(model);
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

            var sch = (from s in db.SchoolClasses
                       join c in db.Class on s.ClassId equals c.Id
                       where s.SchoolId == user.SchoolId && c.ClassNumber == Class && c.ClassLetter == ClassLetter
                       select s).First();
            var mag = (from sm in db.SchoolMagazine
                       join ap in db.AcademicPerformance on sm.Id equals ap.SchoolMagazineId
                       where sm.SchoolClassesId == sch.Id
                       select new SchoolMagazineView
                       {
                           //SubjectId = sm.SubjectId,
                           TeacherId = sm.TeacherId,
                           //Date = sm.Date,
                           SchoolClassesId = sm.SchoolClassesId,
                           SchoolId = sm.SchoolId,
                           PupilId = ap.PupilId,
                           Mark = ap.Mark
                       }).ToList();
            SchoolMagazineViewModel model = new SchoolMagazineViewModel();
            //model.Date = new List<DateTime>();
            model.Pupils_marks = new List<string>();
            foreach (var m in mag)
            {
                
                model.Pupils_marks.Add(m.Mark);
            }



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
            return RedirectToAction("EditMagazineAndView", new { Class = Class, ClassLetter = ClassLetter, daysOfSubjects = daysOfSubjects, Start = start, End = end, Q = quart.QuarterName , model=model, Quarters=Quarters});
        }



        public async Task<IActionResult> ViewMagazine2(string Subject, int Class, string ClassLetter)
        {
            User user = db.Users.Where(i => i.Id == getCurrentUserId()).First();

            var sch = (from s in db.SchoolClasses
                       join c in db.Class on s.ClassId equals c.Id
                       where s.SchoolId == user.SchoolId && c.ClassNumber == Class && c.ClassLetter == ClassLetter
                       select s).First();
            var mag = (from sm in db.SchoolMagazine
                       join ap in db.AcademicPerformance on sm.Id equals ap.SchoolMagazineId
                       where sm.SchoolClassesId == sch.Id
                       select new SchoolMagazineView
                       {
                           //SubjectId = sm.SubjectId,
                           TeacherId = sm.TeacherId,
                           Date = sm.Date.ToLongDateString(),
                           SchoolClassesId = sm.SchoolClassesId,
                           SchoolId = sm.SchoolId,
                           PupilId = ap.PupilId,
                           Mark = ap.Mark
                       }).ToList();

            var dates = (from sm in db.SchoolMagazine
                         join ap in db.AcademicPerformance on sm.Id equals ap.SchoolMagazineId
                         where sm.SchoolClassesId == sch.Id
                         select new SchoolMagazineView
                         {
                             //SubjectId = sm.SubjectId,
                             //TeacherId = sm.TeacherId,
                             Date = sm.Date.ToLongDateString(),
                             //SchoolClassesId = sm.SchoolClassesId,
                             //SchoolId = sm.SchoolId,
                             //PupilId = ap.PupilId,
                             //Mark = ap.Mark
                         }).ToList();
            ViewBag.m = mag;
            ViewBag.dates = dates;


            var comps = db.Academic.FromSqlRaw("exec Procedure_Academic").ToList();
            ViewBag.comps = comps;
            //List<AcademicPerf> result;
            //var query = from t in db.Academic
            //            select t;


            //var groups = from d in db.Academic
            //             group d by d.Date into grp
            //             select grp;
            //groups.Select(g => new 
            //{
            //    Key = g.Select(g => g.PupilId),
            //    Values = g.Select(d2 => new { d2.Mark, d2.Date }).ToArray()
            //}).ToList();

            //var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();

            //var options = optionsBuilder
            //        .UseSqlServer(@"Server=DESKTOP-7SLNJQV;Database=EzineDiary;Trusted_Connection=True;")
            //        .Options;
            //using (var db = new ApplicationContext(options))
            //{
            //    result = await db.Academic.FromSqlRaw("execute Procedure_Academic").ToListAsync();
            //}
            ViewBag.comps = comps;
            return View("ViewMagazine");
        }


            public async Task<IActionResult> SaveMagazine(SchoolMagazineViewModel model, int Class, string ClassLetter)
        {
            Subject subject = db.Subject.Where(s => s.Name == model.Subject).First();
            Employee emp = db.Employee.Where(e => e.UserId == getCurrentUserId()).First();
            var sch = (from s in db.SchoolClasses
                       join c in db.Class on s.ClassId equals c.Id
                       where s.SchoolId == emp.RegistrateSchoolId && c.ClassNumber == Class && c.ClassLetter == ClassLetter
                       select s.Id).ToList();

            Class cl = db.Class.Where(c => c.ClassNumber == Class && c.ClassLetter == ClassLetter).First();

            List<DateTime> dates = model.Date;
            List<string> pupils_marks = model.Pupils_marks;
            List<List<string>> lists = new List<List<string>>();
            for (int i = 0; i < pupils_marks.Count; i += dates.Count + 2)
            {
                lists.Add(pupils_marks.GetRange(i, dates.Count + 2));
            }
            ViewBag.Lists = lists;
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
            int count = schmg.Count();
            await db.SchoolMagazine.AddRangeAsync(schmg);
            await db.SaveChangesAsync();

            var pupIds = db.Pupils.Where(s => s.RegistrateSchoolId == emp.RegistrateSchoolId && s.ClassId == cl.Id).ToList();

            var da = db.SchoolMagazine.OrderByDescending(i => i.Id).Take(count).ToList();
            var dasort = da.OrderBy(s => s.Id).ToList();
            List<AcademicPerformance> acad = new List<AcademicPerformance>();
            for (int i = 0; i < lists.Count(); i++)
            {
                lists[i].RemoveAt(0);
                lists[i].RemoveAt(lists[i].Count - 1);
                for (int j = 0; j < dates.Count(); j++)
                {
                    //if (lists[i][j] != null)
                    //{
                        AcademicPerformance academicPerformance = new AcademicPerformance
                        {
                            PupilId = pupIds[i].Id,
                            Mark = lists[i][j],
                            SchoolMagazineId = dasort[j].Id,
                        };
                        acad.Add(academicPerformance);
                    
                }
            }
            await db.AcademicPerformance.AddRangeAsync(acad);
            await db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
        public IActionResult AddHomework(SchoolMagazineViewModel model, int Class, string ClassLetter, DateTime dates)
        {
            User user = db.Users.Where(i => i.Id == getCurrentUserId()).First();
            var sch = (from s in db.SchoolClasses
                       join c in db.Class on s.ClassId equals c.Id
                       where s.SchoolId == user.SchoolId && c.ClassNumber == Class && c.ClassLetter == ClassLetter
                       select s).First();
           
            IQueryable<SchoolMagazine> sc = db.SchoolMagazine.Where(s => s.Date == dates&&s.SchoolClassesId==sch.Id);
            foreach(var s in sc)
            {
                s.Homework = model.Homework;
            }
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
    }
}
