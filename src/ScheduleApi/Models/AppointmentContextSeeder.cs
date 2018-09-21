using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ScheduleApi.Models
{
    public class AppointmentContextSeeder
    {
        private readonly AppointmentContext _context;
        private readonly UserManager<AppointmentUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AppointmentContextSeeder(
            AppointmentContext context, 
            UserManager<AppointmentUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task EnsureSeedData()
        {

            // Create dummy appointments
            if (!_context.Appointments.Any())
            {
                var sampleAppoint = new Appointment()
                {
                    Name = "Meeting with Dr.Zambo",
                    Comments = "In kings mill hospital with my son",
                    UserName = "",
                    StartTime = new DateTime(2017, 6, 19, 5, 0, 0),
                    EndTime = new DateTime(2017, 6, 19, 6, 0, 0),
                    IsAllDay = false,
                    IsRecurrence = false
                };

                var sampleAppoint2 = new Appointment()
                {
                    Name = "Car MOT",
                    Comments = "In mansfield garage Helsen Bradshaw",
                    UserName = "",
                    StartTime = new DateTime(2017, 5, 1, 4, 0, 0),
                    EndTime = new DateTime(2017, 5, 1, 8, 0, 0),
                    IsAllDay = false,
                    IsRecurrence = false
                };

                _context.Appointments.Add(sampleAppoint);
                _context.Appointments.Add(sampleAppoint2);
                await _context.SaveChangesAsync();
            }

            // Create new "super" user
            var testUser = await _userManager.FindByNameAsync("tester");
            if (testUser == null)
            {
                var newUser = new AppointmentUser()
                {
                    FirstName = "Peter",
                    LastName = "Kujawski",
                    Email = "elusiven@me.com",
                    EmailConfirmed = true,
                    UserName = "tester"
                };

                var userResult = await _userManager.CreateAsync(newUser, "Pracamonika1!");

                // Create administrator role and assign permission to manage accounts
                var adminRole = await _roleManager.FindByNameAsync("administrator");
                if (adminRole == null)
                {
                    adminRole = new IdentityRole("administrator");
                    await _roleManager.CreateAsync(adminRole);
                }

                var userRole = await _roleManager.FindByNameAsync("user");
                if (userRole == null)
                {
                    userRole = new IdentityRole("user");
                    await _roleManager.CreateAsync(userRole);
                }

                if (!await _userManager.IsInRoleAsync(await _userManager.FindByNameAsync("tester"), adminRole.Name))
                {
                    await _userManager.AddToRoleAsync(await _userManager.FindByNameAsync("tester"), adminRole.Name);
                }

                await _userManager.AddClaimAsync(await _userManager.FindByNameAsync("tester"),
                    new Claim(ClaimTypes.Role, "administrator"));

            }
        }
    }
}
