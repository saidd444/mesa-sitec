using Microsoft.EntityFrameworkCore;
using MesaSitec.Data;
var builder = WebApplication.CreateBuilder(args);

// add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//agregar DbContext con SQLite

var connectionString =  builder.Configuration.GetConnectionString("DefaultConnection")
    ??"Data Source=MesaSitec.db";
   builder.Services.AddDbContext<MesaSitecContext>(options =>
    options.UseSqlite(connectionString));

//Cors para el frontend

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();

//use cors
app.UseCors("AllowFrontend");

//config HTTP request
if(app.Environment.IsDevelopment())
 {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
 // aplicar las migraciones y los seed

 using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MesaSitecContext>();
    context.Database.EnsureCreated();
    SeedData.Initialize(context);
}
app.Run();
