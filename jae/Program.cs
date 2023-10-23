using jae.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Office.Interop.Excel;
using jae.Areas.Identity.Data;
using jae.Models;
using System;
using System.Net;
using System.Net.Mail;

public class Program
{
    public static void Main(string[] args)
    {/*
        string fromMail = "dnramos011@gmail.com";
        string fromPassword = "gcagzjnyexpkbuki";

        MailMessage message = new MailMessage();
        message.From = new MailAddress(fromMail);
        message.Subject = "Test Subject";
        message.To.Add(new MailAddress("dnramos011@gmail.com"));
        message.Body = "<html><body>Test Body</body></html>";
        message.IsBodyHtml = true;

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(fromMail, fromPassword),
            EnableSsl = true,

        };

        smtpClient.Send(message);
        */

                var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbConnectionString")));

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<jaeUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.AddControllersWithViews();

        var emailSettings = builder.Configuration.GetSection("EmailSettings");
        builder.Services.Configure<EmailSettings>(emailSettings);




        var app = builder.Build();

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



        app.UseHttpsRedirection();
        app.UseStaticFiles();

        // Place your custom middleware before routing and authentication/authorization
        app.UseMiddleware<RedirectAuthenticatedUsersMiddleware>();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();



        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        app.Run();

    }
    /*
        public static void TestEmailSettings(IConfiguration configuration)
    {
        var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
        var smtpClient = new SmtpClient(emailSettings.SmtpServer)
        {
            Port = emailSettings.SmtpPort,
            Credentials = new NetworkCredential(emailSettings.SmtpUsername, emailSettings.SmtpPassword),
            EnableSsl = false,  // Use SSL if required by the SMTP server
        };

        try
        {
            smtpClient.Send(emailSettings.SmtpUsername, "dnramos011@gmail.com", "Test Subject", "Test Body");
            Console.WriteLine("Email sent successfully."); 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
        }
    }*/
}