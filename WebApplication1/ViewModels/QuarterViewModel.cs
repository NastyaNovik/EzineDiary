using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.ViewModels
{
    public class QuarterViewModel
    {
        public int Id { get; set; }
        public List<string> QuarterName { get; set; }
        public List<DateTime> Start { get; set; }
        public List<DateTime> End { get; set; }
    }
}
