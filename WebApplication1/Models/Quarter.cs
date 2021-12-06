using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Quarter
    {
        public int Id { get; set; }
        public string QuarterName { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
