using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class SchoolMagazine
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public DateTime Date { get; set; }
        public int SchoolId { get; set; }
        public int SchoolClassesId { get; set; }
        public string Homework { get; set; }
    }
}
