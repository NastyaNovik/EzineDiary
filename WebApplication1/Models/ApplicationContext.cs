using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class ApplicationContext : IdentityDbContext<User>
    {
        public DbSet<School> Schools { get; set; }
        public DbSet<RegisteredSchool> RegistratedSchools { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<Position> Position { get; set; }
        public DbSet<Subject> Subject { get; set; }
        public DbSet<Class> Class { get; set; }
        public DbSet<SchoolClasses> SchoolClasses{ get; set; }
        public DbSet<Pupil> Pupils { get; set; }
        public DbSet<Timetable> Timetable { get; set; }
        public DbSet<SchoolMagazine> SchoolMagazine { get; set; }
        public DbSet<AcademicPerformance> AcademicPerformance { get; set; }
        public DbSet<Quarter> Quarter { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();  
        }
    }
}