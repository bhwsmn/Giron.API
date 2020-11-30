using System;
using System.Threading.Tasks;
using AutoMapper;
using Giron.API.DbContexts;
using Giron.API.Entities;
using Giron.API.Models.Constants;
using Giron.API.Services.Classes;
using Giron.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Giron.API
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
            services.AddControllers(options =>
                {
                    // Return HTTP 406 Not Acceptable if Accept header is anything beside application/json or application/xml
                    options.ReturnHttpNotAcceptable = true;
                    
                    // Fixes the routing issue for async controller methods, when using CreatedAtAction() 
                    options.SuppressAsyncSuffixInActionNames = false;
                }).AddXmlDataContractSerializerFormatters()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressMapClientErrors = true;
                });

            services.AddCors();

            services.AddResponseCompression();

            services.AddDbContext<GironContext>(options =>
            {
                options.UseLazyLoadingProxies();
#if DEBUG
                options.UseInMemoryDatabase("Giron");
#else
                options.UseNpgsql(EnvironmentVariables.PostgresqlConnectionString);
#endif

            });

            services.AddDbContext<RefreshTokenLogContext>(options =>
            {
                options.UseInMemoryDatabase("DisabledRefreshTokensDb");
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<GironContext>()
                .AddRoles<IdentityRole>()
                .AddDefaultTokenProviders();

            services.AddAuthorization();
            
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = EnvironmentVariables.JwtIssuer == null,
                        ValidateAudience = EnvironmentVariables.JwtIssuer == null,
                        ValidIssuer = EnvironmentVariables.JwtIssuer ?? "Default",
                        ValidAudience = EnvironmentVariables.JwtAudience ?? "Default",
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(EnvironmentVariables.JwtAccessSecretKey),
                    }
                );

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<IDomainRepository, DomainRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Giron API",
                    Description = "API for the privacy focused minimalistic discussion board - Giron",
                    License = new OpenApiLicense
                    {
                        Name = "GNU General Public License v3.0",
                        Url = new Uri("https://www.gnu.org/licenses/gpl-3.0.en.html"),
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            CreateRoles(serviceProvider).Wait();
            
            // Change this according to production deployment requirements
            app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseResponseCompression();
            
            app.UseSwagger();
            
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Giron API");
                
                c.RoutePrefix = string.Empty;
            });
            
            app.UseRouting();

            app.UseAuthentication(); 
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
        
        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { Roles.User, Roles.Admin };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}