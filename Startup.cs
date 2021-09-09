using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Repositories;
using Catalog.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Catalog
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
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            services.AddSingleton<IMongoClient>(serviceProvider => 
            {
                var settings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                return new MongoClient(settings.ConnectionString);
            });

            //services.AddSingleton<IItemsRepository, InMemItemsRepository>();
            services.AddSingleton<IItemsRepository, MongoDbItemsRepository>();
            
            //services.AddControllers();
            // services.AddControllers().AddJsonOptions( options => 
            // {
            //     options.JsonSerializerOptions.WriteIndented = true;
            // });
            services.AddControllers( options => 
                options.SuppressAsyncSuffixInActionNames = false
            ).AddJsonOptions( options =>
                options.JsonSerializerOptions.WriteIndented = true
            );

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Catalog", Version = "v1" });
            });

            services.AddCors( options => {
                options.AddDefaultPolicy( 
                    builder => builder.AllowAnyOrigin().AllowAnyMethod()
                    // with specify origin "http:.... must not end with '/'
                    // builder => builder.WithOrigins(
                    //     "http://localhost:5001",
                    //     "http://localhost:5100"
                    //     )
                    //     .WithMethods("GET","PUT","POST","DELETE")
                );
                // 
                // options.AddPolicy("mypolicy",
                //     builder => builder.WithOrigins("http://localhost:5001")                    
                // );
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // NOR - middleware CORS must be after userouting() and before useauthorization()
            app.UseCors();  // use default
            //app.UseCors("mypolicy");  // use mypolicy

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
