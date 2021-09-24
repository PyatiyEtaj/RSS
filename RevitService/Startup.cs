using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RevitService.Models;
using RevitService.Providers;
using RevitService.Services;
using RevitService.Services.GoogleServices;
using RevitService.Services.KS.Logic;
using RevitService.Services.KS.Repository;
using RevitService.Services.NeedToLoadLinks;
using RevitService.Services.Repository;
using RevitService.Services.RSO.Logic;
using RevitService.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RevitService
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
            services.Configure<ForgeElementsDatabaseSettings>(Configuration.GetSection(nameof(ForgeElementsDatabaseSettings)))
                .AddSingleton<IForgeElementsDatabaseSettings>(sp => sp.GetRequiredService<IOptions<ForgeElementsDatabaseSettings>>().Value)
                .AddSingleton<IRandomNameProvider, RandomNameProvider>()
                .AddSingleton<IServiceKeyProvider, ServiceKeyProvider>()
                .AddSingleton<IGoogleCredentials, GoogleCredentials>()
                .AddScoped<GoogleDriveService>()
                .AddSingleton<IGoogleRevitDocumentsProvider>( 
                    x => new GoogleSheets(
                        x.GetRequiredService<IGoogleCredentials>(),
                        x.GetRequiredService<GoogleDriveService>(),
                        "1BmSlV6oGXG6NfvjTrpkWnXrPsX9KIsRwI_jnH1SN2V0", 
                        new List<string> {"ְעלמספונא 2!A:G", "ְעלמספונא 3!A:G", "ְעלמספונא 4!A:G" })
                 )
                .AddSingleton<MongoService>()
                .AddHostedService<ConfigureMongoDbService>()
                .AddSingleton(x => new ClientTcpObject(x.GetRequiredService<ILogger<ClientTcpObject>>(), "127.0.0.1", 11000))
                .AddSingleton<IElementRepository, ElementRepository>()
                .AddSingleton<IRevitRequestsRepository, RevitRequestsRepository>()
                .AddSingleton<IKSRepository, KSRepository>()
                .AddSingleton<AKsService, KsFastService>()
                .AddSingleton<IRSOService, RSOByRevitService>()
                .AddSingleton<AuthByForgeOpts>()
                .AddSingleton<IClaimUserProvider, ClaimUserProvider>()
                .AddSingleton<INeedToLoadLinks, LnkLoadChecker>();

            services.AddControllers().AddNewtonsoftJson();
            services.AddHttpClient();

            services.AddAuthentication("AuthByForge")
                    .AddScheme<AuthByForgeOpts, AuthByForge>("AuthByForge", "AuthByForge", opts => { });

            services.AddSignalR().AddNewtonsoftJsonProtocol();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(
                            @"http://bim.progressdk.ru"
                            //@"http://192.168.16.34:8080"
                        )
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RevitService", Version = "v1" });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            if (!Directory.Exists("docs"))
                Directory.CreateDirectory("docs");
            var d = Path.Combine("docs", "temp");
            if (!Directory.Exists(d))
                Directory.CreateDirectory(d);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RevitService v1"));
            }
            app.UseCors()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapHub<RSOHub>("/rsohub");
                });
        }
    }
}
