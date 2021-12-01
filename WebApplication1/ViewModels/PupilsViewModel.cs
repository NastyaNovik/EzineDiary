using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Migrations;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class PupilsViewModel
    {
        [Required]
        [Display(Name = "Имя пользователя")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

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

        [Required]
        public int SchoolId { get; set; }

        [Required]
        public int ClassNumber { get; set; }
        [Required]
        public string ClassLetter { get; set; }

        [Required]
        public string UserId { get; set; }


        public string ImageUrl { get; set; }
    }
}
