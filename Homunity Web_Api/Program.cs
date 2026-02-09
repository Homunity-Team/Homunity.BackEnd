var builder = WebApplication.CreateBuilder(args);

// =======================
// Services
// =======================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()    // 👈 مفتوح لأي Frontend
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// =======================
// Build
// =======================

var app = builder.Build();

// =======================
// Middleware
// =======================

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Homunity API v1");
   
    c.RoutePrefix = string.Empty; // <-- ?? ?????
});



// إجبار HTTPS
app.UseHttpsRedirection();

// CORS
app.UseCors("FrontendPolicy");

// Authorization (حتى لو مفيش Auth دلوقتي)
app.UseAuthorization();

app.MapControllers();

app.Run();