using MailVisitCounter.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace MailVisitCounter
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			string connectionString = Configuration.GetConnectionString("DefaultConnection");
			services.AddDbContext<MSSQlContext>(options => options.UseSqlServer(connectionString));
			services.AddControllers();
			services.AddSwaggerGen(options =>
			{
				var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "MailVisitCounter",
					Description = "API that counts the number of visitors",
					Contact = new OpenApiContact
					{
						Name = "My Github",
						Url = new Uri("https://github.com/RealFoxes/")
					}
				});
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseSwagger(options=> 
			{
				options.SerializeAsV2 = true;
			});

			app.UseSwaggerUI(options =>
			{
				options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
				options.RoutePrefix = string.Empty;
			});

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
