using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class TimetableController : Controller
    {
        private ApplicationContext db;
        public TimetableController(ApplicationContext context)
        {
            db = context;
        }
        public string getCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
        public IActionResult SelectClass()
        {
            ViewBag.SchoolClassesLetters = db.Class.Select(a => a.ClassLetter).Distinct().ToList();
            return View();
        }
        public IActionResult CreateTimetable(int Class, string[] Letters)
        {
            ViewBag.Classes = Class;
            ViewBag.SchoolClasses = Letters;
            return View();
        }
        public async Task<IActionResult> SaveTimetable(TimeTableViewModel model, string[] Letters, int Class)
        {
            List<Timetable> timetableList = new List<Timetable>();
            User user = db.Users.Find(getCurrentUserId());
            var subjects = (from s in db.Subject
                            select s).ToList();
            var teachers = (from s in db.Employee
                            where s.RegistrateSchoolId == user.SchoolId
                            select s).ToList();
            Class ch_class = db.Class.Where(s => s.ClassNumber == Class && s.ClassLetter == model.Letter).First();
            SchoolClasses schoolClasses = db.SchoolClasses.Where(s => s.ClassId == ch_class.Id && s.SchoolId == user.SchoolId).First();
            //model.Subject.RemoveAll(s => string.IsNullOrEmpty(s));
            //model.Teacher.RemoveAll(s => string.IsNullOrEmpty(s));
            //model.DayOfWeek.RemoveAll(s => string.IsNullOrEmpty(s));
            //model.
            for (int i = 0; i < model.Subject.Count(); i++)
            {
                if (model.Subject[i] != null)
                {
                    Timetable timetable = new Timetable
                    {
                        SubjectId = subjects.Where(c => c.Name == model.Subject[i]).Select(c => c.Id).First(),
                        TeacherId = teachers.Where(t => (t.LastName + " " + t.Name.Substring(0, 1) + "." + t.SecondName.Substring(0, 1) + ".") == model.Teacher[i]).Select(t => t.Id).First(),
                        SchoolClassesId = schoolClasses.Id,
                        DayOfWeek = model.DayOfWeek[i],
                        LessonTime = model.LessonTime[i],
                        Cabinet = model.Cabinet[i]
                    };
                    timetableList.Add(timetable);
                }
            }
            await db.Timetable.AddRangeAsync(timetableList);
            await db.SaveChangesAsync();

            return RedirectToAction("CreateTimetable", new { Class = model.Class, Letters = Letters });
        }
    }
}
