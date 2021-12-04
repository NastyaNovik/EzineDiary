using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.ViewModels
{
    public class TimeTableViewModel
    {
        public List<string> Subject { get; set; }
        public List<string> Teacher { get; set; }
        public int Class { get; set; }
        public string Letter { get; set; }
        public List<string> DayOfWeek { get; set; }
        public List<string> LessonTime { get; set; }
        public List<string> Cabinet { get; set; }

    }
}