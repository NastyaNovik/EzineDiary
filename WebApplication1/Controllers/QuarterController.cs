using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class QuarterController : Controller
    {
        private ApplicationContext db;
        public QuarterController(ApplicationContext context)
        {
            db = context;
        }
        [HttpGet]
        public IActionResult Quarter()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Quarter(QuarterViewModel model)
        {
            List<Quarter> add = new List<Quarter>();
            for(int i=0;i<model.QuarterName.Count();i++)
            {
                Quarter quarter = new Quarter
                {
                    QuarterName = model.QuarterName[i],
                    Start = model.Start[i],
                    End = model.End[i]
                };
                add.Add(quarter);
            }
            db.AddRange(add);
            await db.SaveChangesAsync();
            
            return View();
        }
    }
}
