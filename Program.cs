
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Quiz_App_API.Data;
using Quiz_App_API.Data.Models;
using Quiz_App_API.Data.Services;

namespace Quiz_App_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Ne te ardhmen do perdorim Lamar per service configuration ne nje AppRegistry File

            builder.Services.AddControllers();
            //Duhet Cors direkt pas controllerit
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("http://127.0.0.1:5500")  //  frontend URL
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();  // Important for cookies
                });
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                //Konfigurim i Swaggerit qe te mundemi ta testojme API direkt pa postman
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Quiz App API", Version = "v1" });

                // Add security definition and requirements for Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Cookie-based authentication",
                    Name = ".AspNetCore.Identity.Application",
                    In = ParameterLocation.Cookie,
                    Type = SecuritySchemeType.ApiKey
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
            });
            // DbContext setup
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
              builder.Configuration.GetConnectionString("DefaultConnection")
             ));
            //Identity Setup
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
            // Services
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<QuizService>();
            builder.Services.AddScoped<SubjectService>();
            builder.Services.AddScoped<QuizAttemptService>();

            //  Cookie Authentication
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.None; // Important for CORS
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
            });

            //// Add CORS
            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy("AllowAll",
            //        builder =>
            //        {
            //            builder
            //                .AllowAnyOrigin()
            //                .AllowAnyMethod()
            //                .AllowAnyHeader();
            //        });
            //});


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            app.UseHttpsRedirection();
            app.UseCors(); //We need this to use Cors and its policy
            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
