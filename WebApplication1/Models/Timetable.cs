using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Timetable
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public int SchoolClassesId { get; set; }
        public string DayOfWeek { get; set; }
        public string LessonTime { get; set; }
        public string Cabinet { get; set; }
    }
}
