using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ScheduleApi.Data;
using ScheduleApi.Models;

namespace ScheduleApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            _config = builder.Build();
        }

        private readonly IConfigurationRoot _config;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add config service once
            services.AddSingleton(_config);

            // Add db context
            services.AddDbContext<AppointmentContext>();

            // Add appointment's repository
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();

            services.AddTransient<AppointmentContextSeeder>();

            services.AddIdentity<AppointmentUser, IdentityRole>()
                .AddEntityFrameworkStores<AppointmentContext>();

            services.Configure<IdentityOptions>(config =>
            {
                config.Cookies.ApplicationCookie.Events =
                    new CookieAuthenticationEvents()
                    {
                        OnRedirectToLogin = (ctx) =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                            {
                                ctx.Response.StatusCode = 401;
                            }
                            return Task.CompletedTask;
                        },

                        OnRedirectToAccessDenied = (ctx) =>
                        {
                            if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                            {
                                ctx.Response.StatusCode = 403;
                            }
                            return Task.CompletedTask;
                        }
                    };
            });

            // Only for testing purposes, use of allow any origin is dangerous
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));

            // Add framework services.
            services.AddMvc();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthorization(options =>
            {
                // Dashboard is for admins only
                options.AddPolicy("Manage Accounts", policy => policy.RequireRole("administrator"));
                // Policy for resources
                options.AddPolicy("Access Resources", policy => policy.RequireRole("user", "administrator"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, AppointmentContextSeeder seeder)
        {
            app.UseIdentity();

            app.UseJwtBearerAuthentication(new JwtBearerOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = _config["Tokens:Issuer"],
                    ValidAudience = _config["Tokens:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"])),
                    ValidateLifetime = true
                }
            });

            loggerFactory.AddConsole(_config.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors("MyPolicy");

            app.UseMvc();

            seeder.EnsureSeedData().Wait();
        }
    }
}
    
