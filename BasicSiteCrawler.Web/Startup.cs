﻿using BasicSiteCrawler.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BasicSiteCrawler.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
			services.AddMvc();
	        services.AddSignalR();
	        services.AddSingleton<CrawlMediator, CrawlMediator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            

            if (env.IsDevelopment())
            {
	            loggerFactory.AddConsole(LogLevel.Trace);
				app.UseDeveloperExceptionPage();
            }
            else
            {
				loggerFactory.AddConsole();
			}
			
			app.UseStaticFiles();
	        app.UseSignalR(routes =>
	        {
				routes.MapHub<CrawlUrlHub>("/CrawlUrlHub");
	        });

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
			
		}
    }
}
