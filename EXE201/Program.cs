using EXE201.Repository.Implementations;
using EXE201.Repository.Interfaces;
using EXE201.Repository.Models;
using EXE201.Service.Implementation;
using EXE201.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EXE201
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendCors", policy =>
                {
                    var configuredOrigin = builder.Configuration["FrontendSettings:BaseUrl"]?.TrimEnd('/');

                    // Allow configured frontend origin in production; keep common local dev ports for debugging.
                    var origins = new List<string>();
                    if (!string.IsNullOrWhiteSpace(configuredOrigin))
                    {
                        origins.Add(configuredOrigin);
                    }
                    origins.Add("http://localhost:5173");
                    origins.Add("http://localhost:3000");

                    policy.WithOrigins(origins.Distinct(StringComparer.OrdinalIgnoreCase).ToArray())
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            
            // Configure Swagger to support JWT authentication
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            //Register connection string 
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ClothingRentalDbContext>(option => option.UseSqlServer(connectionString));

            // Configure JWT Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
                        ValidAudience = builder.Configuration["AppSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!))
                    };
                });

            builder.Services.AddAuthorization();

            // Repository Layer
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Service Layer
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IEmailService, SmtpEmailService>();
            builder.Services.AddScoped<IWishlistService, WishlistService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IReviewImageService, ReviewImageService>();
            builder.Services.AddScoped<IOutfitImageService, OutfitImageService>();
            builder.Services.AddScoped<IOutfitSizeService, OutfitSizeService>();
            builder.Services.AddScoped<IOutfitService, OutfitService>();
            builder.Services.AddScoped<IOutfitAttributeService, OutfitAttributeService>();
            builder.Services.AddScoped<IRentalPackageService, RentalPackageService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<IServicePackageService, ServicePackageService>();
            builder.Services.AddScoped<IServiceBookingService, ServiceBookingService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IStudioService, StudioService>();
            builder.Services.AddScoped<ILoyaltyTransactionService, LoyaltyTransactionService>();
            builder.Services.AddScoped<IServiceAddonService, ServiceAddonService>();
            builder.Services.AddScoped<IPayOsService, PayOsService>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<ILeonardoAIService, LeonardoAIService>();
            // Caching
            builder.Services.AddMemoryCache();

            // Mapper
            builder.Services.AddAutoMapper(typeof(EXE201.Service.Mapper.MappingProfile));

           var app = builder.Build();

var enableSwagger = app.Environment.IsDevelopment()
    || app.Configuration.GetValue<bool>("Swagger:Enabled");

            if (enableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("FrontendCors");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
