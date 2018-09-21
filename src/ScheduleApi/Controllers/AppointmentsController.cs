using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScheduleApi.Data;
using ScheduleApi.Models;
using System.Security.Claims;

namespace ScheduleApi.Controllers
{
    [Route("api/[controller]")]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentRepository _repository;
        private readonly ILogger<AppointmentsController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppointmentsController(IAppointmentRepository repository, ILogger<AppointmentsController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("")]
        public IActionResult Get()
        {
            try
            {
                var myUser = HttpContext.User.Identity.Name;
                var appointments = _repository.GetAllAppointments();
                if (appointments == null) return NotFound();
                return Ok(appointments);
            }
            catch
            {
                _logger.LogError("Failed to get appointments");
            }

            return BadRequest();
        }

        [HttpGet("{id}", Name = "AppointmentGet")]
        public IActionResult GetById(int id)
        {
            try
            {
                var appointment = _repository.GetAppointment(id);
                if (appointment == null) return NotFound($"Camp {id} was not found");
                return Ok(appointment);
            }
            catch
            {
                _logger.LogError("Failed to get this appointment");
            }

            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Appointment model)
        {
            try
            {
                _repository.Add(model);

                if (await _repository.SaveAllAsync())
                {
                    var newUri = Url.Link("AppointmentGet", new {id = model.Id});
                    return Created(newUri, model);
                }
                else
                {
                    _logger.LogWarning("Could not save appointment to the database");
                }
            }
            catch(Exception ex)
            {
               _logger.LogError($"Critical error when saving new appointment, Ex: {ex}"); 
            }

            return BadRequest();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Appointment model)
        {
            try
            {
                // Get old appointment
                var oldAppointment = _repository.GetAppointment(id);
                if (oldAppointment == null) return NotFound($"Could not find appointment with id {id}");

                // Map model to old appointment
                oldAppointment.Name = model.Name ?? oldAppointment.Name;
                oldAppointment.Comments = model.Comments ?? oldAppointment.Comments;
                oldAppointment.IsAllDay = model.IsAllDay;
                
                // Save changes in the database
                if (await _repository.SaveAllAsync())
                {
                    return Ok(oldAppointment);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($" {ex}");
            }

            return BadRequest("Could not update appointment");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var appointment = _repository.GetAppointment(id);
                if (appointment == null) return NotFound($"Could not find appointment with id of {id}");

                _repository.Delete(appointment);

                if (await _repository.SaveAllAsync())
                {
                    return Ok();
                }
                else
                {
                    _logger.LogError("Could not delete an appointment");
                }
            }
            catch (Exception ex)
            {

            }

            return BadRequest("Could not delete appointment");
        }

    }
}
