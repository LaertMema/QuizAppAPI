
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
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
            // Add Services
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<QuizService>();
            builder.Services.AddScoped<SubjectService>();
            builder.Services.AddScoped<QuizAttemptService>();

            // Add Cookie Authentication
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            });

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
