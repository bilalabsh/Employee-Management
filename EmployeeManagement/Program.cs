using EmployeeManagement.Core.Interfaces;
using EmployeeManagement.Core.Mappings;
using EmployeeManagement.Core.Services;
using EmployeeManagement.Domain.Data;
using EmployeeManagement.Domain.Interfaces;
using EmployeeManagement.Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using static EmployeeManagement.Core.Services.EmployeeService; // ✅ Correct if needed

using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Infrastructure.Repositories;

// Ensure roles cd eexist in the database
static async Task EnsureRolesCreated(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roleNames = { "Admin", "Employee" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

var builder = WebApplication.CreateBuilder(args);

// 🔹 Register Identity Services
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>() 
    .AddDefaultTokenProviders();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
//service for password hashing
builder.Services.AddScoped<IPasswordHasher<IdentityUser>, PasswordHasher<IdentityUser>>();
builder.Services.AddScoped<IPasswordHasher<Employee>, PasswordHasher<Employee>>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();



// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

// 🔹 Add Authentication
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
 
        ClockSkew = TimeSpan.Zero 
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
                context.Response.StatusCode = 401; 
                return context.Response.WriteAsync("Token has expired. Please refresh or log in again.");
            }
            return Task.CompletedTask;
        }
    };
});

// 🔹 Add Authorization
builder.Services.AddAuthorization();

builder.Services.AddControllers();

// 🔹 Register JWT Service
builder.Services.AddSingleton<JwtService>();

// 🔹 Register Swagger with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Employee Management API", Version = "v1" });

    // Enable JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}' (without quotes). Example: Bearer abcdef123456"
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
            new string[] {}
        }
    
    });
});

// 🔹 Register Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

// 🔹 Register AutoMapper
builder.Services.AddAutoMapper(typeof(EmployeeMappingProfile));

// 🔹 Register Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Ensure roles exist on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await EnsureRolesCreated(services);
}

// Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<JwtMiddleware>();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseAuthentication(); // 🔹 Make sure this comes BEFORE authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
