using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScheduleApi.Models;

namespace ScheduleApi.Data
{
    public interface IAppointmentRepository
    {
        // Basic DB Operations
        void Add<T>(T entity) where T : class;
        void Delete<T>(T entity) where T : class;
        Task<bool> SaveAllAsync();

        // Appointments 
        IEnumerable<Appointment> GetAllAppointments();
        IEnumerable<Appointment> GetAllAppointmentsByStartDate(String currentDate, String currentView, String currentAction);
        Appointment GetAppointment(int id);

    }
}