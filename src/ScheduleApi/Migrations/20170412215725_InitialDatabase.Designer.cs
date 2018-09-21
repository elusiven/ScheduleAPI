using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ScheduleApi.Models;

namespace ScheduleApi.Migrations
{
    [DbContext(typeof(AppointmentContext))]
    [Migration("20170412215725_InitialDatabase")]
    partial class InitialDatabase
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ScheduleApi.Models.Appointment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Comments");

                    b.Property<DateTime>("EndTime");

                    b.Property<bool>("IsAllDay");

                    b.Property<bool>("IsRecurrence");

                    b.Property<string>("Name");

                    b.Property<string>("RecurrenceRule");

                    b.Property<DateTime>("StartTime");

                    b.Property<string>("UserName");

                    b.HasKey("Id");

                    b.ToTable("Appointments");
                });
        }
    }
}
