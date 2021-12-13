using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
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
        IWebHostEnvironment _hostEnvironment;
        public TimetableController(ApplicationContext context, IWebHostEnvironment hostEnvironment)
        {
            db = context;
            _hostEnvironment = hostEnvironment;
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
        public IActionResult CreateTimetable(int Class, string[] Letters, string Error)
        {
            ViewBag.Error = Error;
            User user = db.Users.Find(getCurrentUserId());
            var teachers = (from s in db.Employee
                            where s.RegistrateSchoolId == user.SchoolId
                            select s).ToList();
            ViewBag.teach = teachers;
            ViewBag.Classes = Class;
            var timet = (from s in db.SchoolClasses
                         join cl in db.Class on s.ClassId equals cl.Id
                         join t in db.Timetable on s.Id equals t.SchoolClassesId
                         where Letters.Contains(cl.ClassLetter) && cl.ClassNumber == Class && s.SchoolId == user.SchoolId
                         select cl.ClassLetter).ToList().Distinct();
            List<string> let = new List<string>();
            foreach (var l in Letters)
            {
                let.Add(l);
            }
            foreach (var t in timet)
            {
                let.Remove(t);
            }
            ViewBag.SchoolClasses = let;
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
            int count = db.SchoolClasses.Where(s => s.ClassId == ch_class.Id && s.SchoolId == user.SchoolId).Count();
            if (count != 0)
            {
                SchoolClasses schoolClasses = db.SchoolClasses.Where(s => s.ClassId == ch_class.Id && s.SchoolId == user.SchoolId).First();

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
                savePdf(user.SchoolId, Class, model.Letter);
                return RedirectToAction("LoadTimeTable", new { SchoolId = user.SchoolId });
            }
            else
            {
                string Error = "Этот класс еще не зарегистрирован!";
                return RedirectToAction("CreateTimetable", new { Class = Class, Letters = Letters, Error = Error });
            }

        }

        public void savePdf(int SchoolId, int Class, string Letter)
        {
            var timetable = (from a in db.Timetable
                             join e in db.Employee on a.TeacherId equals e.Id
                             join s in db.Subject on a.SubjectId equals s.Id
                             join sc in db.SchoolClasses on a.SchoolClassesId equals sc.Id
                             join cl in db.Class on sc.ClassId equals cl.Id
                             where sc.SchoolId == SchoolId && cl.ClassNumber == Class && cl.ClassLetter == Letter
                             select new
                             {
                                 DayOfWeek = a.DayOfWeek,
                                 Subject = s.Name,
                                 LessonTime = a.LessonTime,
                                 Teacher = e.LastName + " " + e.Name.Substring(0, 1) + "." + e.SecondName.Substring(0, 1) + ".",
                                 Cabinet = a.Cabinet,
                                 Class = cl.ClassNumber + "'" + cl.ClassLetter + "'"
                             }).ToList();
            iTextSharp.text.Document doc = new iTextSharp.text.Document();
            PdfWriter.GetInstance(doc, new FileStream(_hostEnvironment.WebRootPath + $"/timetable/{SchoolId}_{Class}_{Letter}.pdf", FileMode.Create));
            doc.Open();
            _hostEnvironment.WebRootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            BaseFont baseFont = BaseFont.CreateFont(_hostEnvironment.WebRootPath + "/fonts/ARIAL.TTF", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, iTextSharp.text.Font.DEFAULTSIZE, iTextSharp.text.Font.NORMAL);
            PdfPTable table = new PdfPTable(5);
            PdfPCell cell = new PdfPCell(new Phrase($"Расписание для {Class} '{Letter}'", font));
            cell.Colspan = 6;
            cell.HorizontalAlignment = 1;
            cell.Border = 0;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase("День недели", font));
            cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Предмет", font));
            cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Учитель", font));
            cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Время урока", font));
            cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Phrase("Кабинет", font));
            cell.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            for (int i = 0; i < timetable.Count(); i++)
            {

                table.AddCell(new Phrase(timetable[i].DayOfWeek, font));
                table.AddCell(new Phrase(timetable[i].Subject, font));
                table.AddCell(new Phrase(timetable[i].Teacher, font));
                table.AddCell(new Phrase(timetable[i].LessonTime, font));
                table.AddCell(new Phrase(timetable[i].Cabinet, font));

            }
            doc.Add(table);
            doc.Close();
        }

        public IActionResult LoadTimeTable(int SchoolId)
        {
            DirectoryInfo di = new DirectoryInfo(_hostEnvironment.WebRootPath + "/timetable");
            FileInfo[] rgFile = di.GetFiles().Where(s => s.Name.Contains($"{SchoolId}")).ToArray();
            ViewBag.filesToLoad = rgFile;
            return View();
        }
    }
}
