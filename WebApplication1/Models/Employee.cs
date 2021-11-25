using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string LastName { get; set; }

        public string Name { get; set; }

        public string SecondName { get; set; }

        public string PhoneNumber { get; set; }

        public string AdditionalInformation { get; set; }

        public int RegistrateSchoolId { get; set; }

        public int PositionId { get; set; }

        public string UserId { get; set; }

        public string Subject { get; set; }

        public string ImageUrl { get; set; }
    }
}