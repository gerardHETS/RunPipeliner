using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RunPipeliner.Api;
using RunPipeliner.Business;
using RunPipeliner.Common.Mapper;
using RunPipeliner.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RunPipeliner
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
            services.AddControllers();
            services.AddSwaggerGen();


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RunPipeliner.API", Version = "v1" });
            });

            //Registering DI for interfaces that ends with "Access" from filtered assemblies by using Scrutor library
            services.Scan(scan =>
               scan.FromCallingAssembly()
                   .FromAssemblies(
                       typeof(IBusinessAccessAssembly).Assembly,
                       typeof(IDataAccessAssembly).Assembly
                   )
                   .AddClasses(@class =>
                       @class.Where(type =>
                           !type.Name.StartsWith('I')
                           && type.Name.EndsWith("Access")
                       ))
                   .AsMatchingInterface());

            // Auto Mapper Configurations
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new ApiMappings());
                mc.AddProfile(new BusinessMappings());
                mc.AddProfile(new DataAccessMappings());

            });

            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "EquitableRostering.API v1");
                c.RoutePrefix = string.Empty;
            });

            AutoMapperConfig.Initialize();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Index}");
            });
        }
    }
}
