using L.R._Gcaleka__Co.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Google.Cloud.Storage.V1;
using System;
using L.R._Gcaleka__Co.Models;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Hangfire;
using Hangfire.SqlServer;
using L.R._Gcaleka__Co;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var smtpSettings = new SmtpSettings();
        builder.Configuration.GetSection("SmtpSettings").Bind(smtpSettings);
        builder.Services.AddSingleton(smtpSettings);
        builder.Services.AddSingleton<IEmailSender, EmailServices>();

        builder.Services.AddHangfire(config => config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

        builder.Services.AddHangfireServer();
        builder.Services.AddHostedService<UnreviewedFilesService>();
        builder.Services.AddSingleton<GcsSignedUrlService>();

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddSingleton(StorageClient.Create());
        var googleCredentialsPath = builder.Configuration["GoogleCloud:CredentialsPath"];
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "googleCredentialsPath");

        builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.AddControllersWithViews();
    
        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { "Admin", "Client", "CEO", "Candidate Attorney" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            string email = "admin@admin.com";
            string password = "Test1284,";

            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = "Admin",
                    LastName ="Admin",
                    //MustChangePassword =  true,
                    EmployeeNumber = "LGC-1001"

                    //Add Id Number Application User
                };

                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }

        //RecurringJob.AddOrUpdate<ReminderService>(
        //    "send-reminder",
        //    x => x.SendAppointmentReminders(),
        //    Cron.Daily);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //app.UseHangfireDashboard("/hangfire", new DashboardOptions{
        //Authorization = new[]{new HangfireAuthorizationFilter()}
        //);
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();
    }


}
