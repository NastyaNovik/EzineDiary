using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class AcademicPerformance
    {
        public int Id { get; set; }
        public int PupilId { get; set; }
        public string Mark { get; set; }
        public int SchoolMagazineId { get; set; }
    }
}
