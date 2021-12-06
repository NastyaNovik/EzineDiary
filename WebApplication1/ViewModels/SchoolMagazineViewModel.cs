using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.ViewModels
{
    public class SchoolMagazineViewModel
    {
        public List<DateTime> Date { get; set; }
        public List<string> Pupils_marks { get; set; }
        public string Subject { get; set; }
        public int Class { get; set; }
        public string ClassLetter { get; set; }
        public string Homework { get; set; }
    }
}
