using Hangfire;
using Microsoft.EntityFrameworkCore;
using WebAPIWithHangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Create connection string
var ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//Add database service
builder.Services.AddDbContext<AppDbContext>(options =>
{ options.UseSqlServer(ConnectionString ?? throw new InvalidOperationException("Connection string not found")); });

// Adding Hangfire service
builder.Services.AddHangfire(sp => { sp.UseSqlServerStorage(ConnectionString); });
builder.Services.AddHangfireServer();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseHangfireDashboard();
app.Run();
