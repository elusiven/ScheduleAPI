using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScheduleApi.Models;

namespace ScheduleApi.Data
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppointmentContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppointmentRepository(AppointmentContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<bool> SaveAllAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }

        public IEnumerable<Appointment> GetAllAppointments()
        {
            var userName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return _context.Appointments.Where(a => a.UserName == userName).ToList();
        }

        public IEnumerable<Appointment> GetAllAppointmentsByStartDate(String currentDate, String currentView, String currentAction)
        {
            var userName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return _context.Appointments
                .Where(a => a.UserName == userName)
                .OrderBy(a => a.StartTime)
                .ToList();
        }

        public Appointment GetAppointment(int id)
        {
            var userName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            return _context.Appointments
                .Where(a => a.UserName == userName)
                .FirstOrDefault(a => a.Id == id);
        }

    }
}
