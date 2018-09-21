using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ScheduleApi.Models
{
    public class AppointmentContext : IdentityDbContext<AppointmentUser>
    {
        private readonly IConfigurationRoot _config;

        public AppointmentContext(IConfigurationRoot config, DbContextOptions options) 
            : base(options)
        {
            _config = config;
        }

        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(_config["Data:AppointmentContextConnection"]);
        }
    }
}
