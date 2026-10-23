using atlasTaskflow.Data;
using atlasTaskflow.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace atlasTaskflow.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
    

        // add link to db tables
        // if your variable name here is getting error. stooopid ass model wasn't made
        // public. dumbahhhhh.
        public DbSet<InstructorModel> Instructors { get; set; }
        public DbSet<CourseModel> Courses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // reference to actual db table
            builder.Entity<InstructorModel>()
                .ToTable("Instructors");

            builder.Entity<CourseModel>()
                .ToTable("Courses");
        }
    }
}
