using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ScheduleApi.Models;
using ScheduleApi.ViewModel;

namespace ScheduleApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "Manage Accounts")]
    public class IdentityController : Controller
    {
        private readonly UserManager<AppointmentUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppointmentUser> _signInManager;
        private readonly ILogger<IdentityController> _logger;
        private readonly IPasswordHasher<AppointmentUser> _hasher;
        private readonly IConfigurationRoot _config;

        public IdentityController(
            UserManager<AppointmentUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            SignInManager<AppointmentUser> signInManager,
            ILogger<IdentityController> logger,
            IPasswordHasher<AppointmentUser> hasher,
            IConfigurationRoot config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
            _hasher = hasher;
            _config = config;
        }

        // Get All Users
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var role = await _roleManager.FindByNameAsync("user");
                var users = await _userManager.GetUsersInRoleAsync(role.Name);

                return new JsonResult(users);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not get all users, Ex: {ex}");
            }

            return BadRequest("Could not get all users");
        }

        // Create a user
        [HttpPost("Create")]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateViewModel model)
        {
            try
            {
                var user = new AppointmentUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    AccessFailedCount = 0,
                    Email = model.Email,
                    EmailConfirmed = false,
                    LockoutEnabled = true,
                    NormalizedEmail = model.Email.ToUpper(),
                    NormalizedUserName = model.UserName.ToUpper(),
                    TwoFactorEnabled = false,
                    UserName = model.UserName
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await AddToRole(model.UserName, "user");
                    await AddClaims(model.UserName);
                    return Ok();
                }
                else
                {
                    return BadRequest(result.Errors.ToArray());
                }

                return new JsonResult(result.Errors);
            }
            catch (Exception ex)
            {
               _logger.LogError($"Could not create new user, Ex: {ex}");
            }

            return BadRequest("Could not create a new user");
        }

        // Delete a user
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromBody] string username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                var result = await _userManager.DeleteAsync(user);

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not delete a user, Ex: {ex}");
            }

            return BadRequest("Could not delete a user");
        }

        // Add role to a user
        private async Task AddToRole(string userName, string roleName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            await _userManager.AddToRoleAsync(user, roleName);
        }

        // Add claims to a user
        private async Task AddClaims(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            var claims = new List<Claim>
            {
                new Claim(type: "role", value: "user"),
                new Claim(type: JwtRegisteredClaimNames.Email, value: user.Email)
            };
            await _userManager.AddClaimsAsync(user, claims);
        }

        // Generate JWT Token
        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<IActionResult> CreateToken([FromBody]CredentialModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    if (_hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) ==
                        PasswordVerificationResult.Success)
                    {
                        var userClaims = await _userManager.GetClaimsAsync(user);

                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName), 
                            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                            new Claim(JwtRegisteredClaimNames.Email, user.Email)
                        }.Union(userClaims);

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            issuer: _config["Tokens:Issuer"],
                            audience: _config["Tokens:Audience"],
                            claims: claims,
                            expires: DateTime.UtcNow.AddMinutes(15),
                            signingCredentials: creds
                        );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        });
                    }
                }
                else
                {
                    return BadRequest(Json("Credentials are incorrect").Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception thrown while logging in: {ex}");
            }

            return BadRequest("Failed to generate token");
        }
    }
}
