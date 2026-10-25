using atlasTaskflow.Data;
using atlasTaskflow.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace atlasTaskflow.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {


        // add link to db tables
        // if your variable name here is getting error. stooopid ass model wasn't made
        // public. dumbassss.
        public DbSet<InstructorModel> Instructors { get; set; }
        public DbSet<CourseModel> Courses { get; set; }
        public DbSet<OrganizationModel> Organizations { get; set; }
        public DbSet<ClassroomModel> Classrooms { get; set; }
        public DbSet<EnrollmentModel> Enrollments { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<AssignmentModel> Assignments { get; set; }
        public DbSet<StudentAssignmentModel> StudentAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // reference to actual db table
            builder.Entity<InstructorModel>()
                .ToTable("Instructors");

            builder.Entity<InstructorModel>()
                .HasIndex(x => x.UserId)
                .IsUnique()
                .HasFilter("[UserId] IS NOT NULL");

            builder.Entity<CourseModel>()
                .ToTable("Courses");

            builder.Entity<OrganizationModel>().ToTable("Organizations");
            builder.Entity<ClassroomModel>().ToTable("Classrooms");
            builder.Entity<EnrollmentModel>().ToTable("Enrollments");
            builder.Entity<TaskModel>().ToTable("Tasks");
            builder.Entity<AssignmentModel>().ToTable("Assignments");
            builder.Entity<StudentAssignmentModel>().ToTable("StudentAssignments");

            builder.Entity<AssignmentModel>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<OrganizationModel>()
                .HasMany(x => x.Instructors)
                .WithOne(x => x.Organization)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ApplicationUser>()
                .HasOne(x => x.Organization)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<OrganizationModel>()
                .HasMany(x => x.Classrooms)
                .WithOne(x => x.Organization)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InstructorModel>()
                .HasMany(x => x.Classrooms)
                .WithOne(x => x.Instructor)
                .HasForeignKey(x => x.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClassroomModel>()
                .HasMany(x => x.Enrollments)
                .WithOne(x => x.Classroom)
                .HasForeignKey(x => x.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClassroomModel>()
                .HasMany(x => x.Assignments)
                .WithOne(x => x.Classroom)
                .HasForeignKey(x => x.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AssignmentModel>()
                .HasMany(x => x.StudentAssignments)
                .WithOne(x => x.Assignment)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EnrollmentModel>()
                .HasIndex(x => new { x.StudentId, x.ClassroomId })
                .IsUnique();

            builder.Entity<StudentAssignmentModel>()
                .HasIndex(x => new { x.StudentId, x.AssignmentId })
                .IsUnique();
        }
    }
}
