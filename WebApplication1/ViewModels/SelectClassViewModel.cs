using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.ViewModels
{
    public class SelectClassViewModel
    {

        [Required]
        public int Class { get; set; }
        [Required]
        public string ClassLetter { get; set; }
    }
}
