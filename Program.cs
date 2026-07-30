using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MesaSitec.Data;
using MesaSitec.Services;

var builder = WebApplication.CreateBuilder(args);

// Registrar servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// DbContext con SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=MesaSitec.db";
builder.Services.AddDbContext<MesaSitecContext>(options =>
    options.UseSqlite(connectionString));

// AuthService
builder.Services.AddScoped<AuthService>();

// JWT
var jwtKey = "Tu-clave-es-muy-larga-de-minimo-32-caracteres-para-produccion";
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
                NameClaimType = "sub"  
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Swagger con autenticación JWT
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Ingresa tu JWT"
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

var app = builder.Build();

// Middleware
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    var auth = context.Request.Headers["Authorization"].ToString();
    System.Console.WriteLine($"Authorization header: {auth}");
    await next();
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MesaSitecContext>();
    context.Database.EnsureCreated();
    SeedData.Initialize(context);
}

app.Run();