using Homunity_Buisness_Logic;
using Homunity_Business_Logic;
using Homunity_Data_Access;
using Homunity_Data_Access.Data;
using Homunity_Data_Access.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(builder.Environment.IsDevelopment() ? LogLevel.Information : LogLevel.Warning);
// =======================
// Secrets & Configuration (Sprint 4)
// =======================

var connectionString = builder.Configuration.GetConnectionString("HomunityDb")
    ?? throw new InvalidOperationException("ConnectionStrings:HomunityDb غير موجودة. اضبطها عبر User Secrets.");
clsDataAccessSettings.Initialize(connectionString);

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key غير موجود. اضبطه عبر User Secrets.");

// =======================
// Services
// =======================
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddScoped<clsChat>();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Homunity API",
        Version = "v1",
        Description = "RESTful API for the Homunity real-estate rental platform."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "اكتب: Bearer {token}"
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

// ===== EF Core =====
builder.Services.AddDbContext<HomunityDbContext>(options =>
    options.UseSqlServer(clsDataAccessSettings.ConnectionString));

// ===== Users =====
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUsersService, UsersService>();

// ===== Auth =====
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ===== Properties =====
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IPropertyOrchestratorService, PropertyOrchestratorService>();

// ===== JWT Authentication =====
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ===== CORS (Sprint 4 — مقيّد بدل AllowAnyOrigin) =====
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .AllowAnyHeader();
    });
});

// ===== Rate Limiting (Sprint 4) =====
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("UploadPolicy", opt =>
    {
        opt.PermitLimit = 20;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

// =======================
// Build
// =======================

var app = builder.Build();

// =======================
// Middleware
// =======================

// Exception Handling (Sprint 4) — أول حاجة في الـPipeline
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = feature?.Error;

        context.Response.ContentType = "application/problem+json";

        ProblemDetails problem;

        switch (exception)
        {
            case Homunity_Buisness_Logic.Exceptions.NotFoundException notFoundEx:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                problem = new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = notFoundEx.Message,
                    Status = StatusCodes.Status404NotFound
                };
                break;

            case Homunity_Buisness_Logic.Exceptions.ConflictException conflictEx:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                problem = new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = conflictEx.Message,
                    Status = StatusCodes.Status409Conflict
                };
                break;

            case Homunity_Buisness_Logic.Exceptions.ValidationAppException validationEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                problem = new ValidationProblemDetails(validationEx.Errors)
                {
                    Title = "Validation Failed",
                    Status = StatusCodes.Status400BadRequest
                };
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                problem = new ProblemDetails
                {
                    Title = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError,
                    // التفاصيل الكاملة تظهر بس في Development — نفس مبدأ Sprint 4
                    Detail = app.Environment.IsDevelopment() ? exception?.ToString() : null
                };
                break;
        }

        await context.Response.WriteAsJsonAsync(problem);
    });
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Homunity API v1");
});

app.UseHttpsRedirection();
app.UseStaticFiles();

// Security Headers (Sprint 4)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();

public partial class Program { }
