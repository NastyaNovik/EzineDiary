using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class SchoolMagazineView
    {
        public string Subject { get; set; }
        public int TeacherId { get; set; }
        public string Date { get; set; }
        public int SchoolId { get; set; }
        public int SchoolClassesId { get; set; }
        public int PupilId { get; set; }
        public string Mark { get; set; }
        public string Homework { get; set; }
    }
}
