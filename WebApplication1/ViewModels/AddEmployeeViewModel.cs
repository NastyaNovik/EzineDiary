using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.ViewModels
{
    public class AddEmployeeViewModel
    {
        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Имя")]
        public string Name { get; set; }

        [Display(Name = "Отчество")]
        public string SecondName { get; set; }

        [Required]
        [Display(Name = "Номер телефона")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Дополнительная информация")]
        public string AdditionalInformation { get; set; }

        [Required]
        public int SchoolId { get; set; }

        [Required]
        public string Position { get; set; }

        [Required]
        public int UserId { get; set; }

        public string Subject { get; set; }

        public string ImageUrl { get; set; }
    }
}
