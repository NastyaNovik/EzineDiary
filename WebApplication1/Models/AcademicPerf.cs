using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    [Keyless]
    public class AcademicPerf
    {
        public int PupilId { get; set; }
        public string Mark { get; set; }
        public DateTime Date { get; set; }
    }
}
