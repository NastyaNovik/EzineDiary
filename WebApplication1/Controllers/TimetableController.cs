using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IActionResult SelectClass()
        {
            ViewBag.SchoolClassesLetters = db.Class.Select(a=>a.ClassLetter).Distinct().ToList();
            return View();
        }
        public IActionResult CreateTimetable(int Class, string [] Letters)
        {
            ViewBag.Classes = Class;
            ViewBag.SchoolClasses = Letters;
            return View();
        }
    }
}
