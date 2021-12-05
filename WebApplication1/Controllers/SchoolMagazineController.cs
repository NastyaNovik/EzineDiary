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
    public class SchoolMagazineController:Controller
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
        public IActionResult SchoolMagazine(int ClassNumber, string ClassLetter)
        {
            return View();
        }
    }
}
