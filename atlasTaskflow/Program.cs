
using atlasTaskflow.Data;
using atlasTaskflow.Models;
using atlasTaskflow.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace atlasTaskflow
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database connection
            var connectionString = builder.Configuration
                .GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Identity configuration
            builder.Services
                .AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddScoped<
                IUserClaimsPrincipalFactory<ApplicationUser>,
                ApplicationUserClaimsPrincipalFactory>();

            // Authorization policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly",
                    policy => policy.RequireRole("Admin"));

                options.AddPolicy("InstructorOnly",
                    policy => policy.RequireRole("Instructor"));

                options.AddPolicy("StudentOnly",
                    policy => policy.RequireRole("Student"));
            });

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Apply migrations and initialize admin
            await SeedAdminAsync(app.Services);

            // HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }

        private static async Task SeedAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            // Apply existing migrations
            await dbContext.Database.MigrateAsync();

            // Ensure roles exist
            string[] roles = { "Admin", "Instructor", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Unable to create role {role}: " +
                            string.Join("; ", result.Errors.Select(
                                error => error.Description)));
                    }
                }
            }

            // Demonstration admin account
            const string email = "admin@admin.com";
            const string password = "Abc_123";

            var admin = await userManager.FindByEmailAsync(email);

            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    UserType = "Admin"
                };

                var createResult = await userManager.CreateAsync(
                    admin, password);

                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Unable to create admin: " +
                        string.Join("; ", createResult.Errors.Select(
                            error => error.Description)));
                }
            }
            else
            {
                admin.UserName = email;
                admin.Email = email;
                admin.EmailConfirmed = true;
                admin.UserType = "Admin";

                var updateResult = await userManager.UpdateAsync(admin);

                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Unable to update admin: " +
                        string.Join("; ", updateResult.Errors.Select(
                            error => error.Description)));
                }

                // Reset demonstration password
                if (await userManager.HasPasswordAsync(admin))
                {
                    var removeResult =
                        await userManager.RemovePasswordAsync(admin);

                    if (!removeResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            "Unable to remove admin password.");
                    }
                }

                var passwordResult =
                    await userManager.AddPasswordAsync(admin, password);

                if (!passwordResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Unable to reset admin password: " +
                        string.Join("; ", passwordResult.Errors.Select(
                            error => error.Description)));
                }
            }

            // Ensure admin belongs to Admin role
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                var roleResult =
                    await userManager.AddToRoleAsync(admin, "Admin");

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Unable to assign Admin role: " +
                        string.Join("; ", roleResult.Errors.Select(
                            error => error.Description)));
                }
            }
        }
    }
}
