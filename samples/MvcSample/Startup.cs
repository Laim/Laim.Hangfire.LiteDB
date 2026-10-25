using System;
using System.Diagnostics;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MvcSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHangfire(configuration =>
                configuration.UseLiteDbStorage(Configuration["ConnectionStrings:Database"]));
            services.AddHangfireServer();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "Development")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            // Demo only: the Hangfire dashboard is not authenticated here.
            // Do not expose this sample publicly without adding authorisation.
            app.UseHangfireDashboard();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            // these are fire and forget, they run once on startup and then never again
            // i.e if you wanted to send an email on startup that said "XYZ is booted" 
            // or something like that
            BackgroundJob.Enqueue(() => Console.WriteLine("Background Job: Hello, world!"));
            BackgroundJob.Enqueue(() => Test());

            // depending on your workflow, it may be worth wrapping recurring jobs in a if statement
            // so that they only run in production and don't run in dev
            // example:
            //if(env.IsDevelopment())
            //{
            //    var recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();
            //    recurringJobManager.AddOrUpdate("prod-database-backup", () => Test(), "* * * * *");
            //}

            var recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();

            // run the job every minute, if you don't understand cron you can use https://cronguru.com
            // reconisiable-job-id shows up in the hangfire dashboard, so name it something you'll recognize such as
            // weekly-db-backup, monthly-statistics-report, etc.
            recurringJobManager.AddOrUpdate("recognizable-job-id", () => Test(), "* * * * *");

        }

        public static int X;

        // when this runs, it'll try twice before it gets nuked
        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public static void Test()
        {
            Debug.WriteLine($"{X++} Cron Job: Hello, world!");
        }
    }
}
