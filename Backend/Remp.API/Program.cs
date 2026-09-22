using Microsoft.EntityFrameworkCore;
using Remp.DataAccess.Data;
using Remp.API.Services.Email;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    /**app.MapPost("/api/email/test", async (IEmailSender emailSender) =>
    {
        await emailSender.SendEmailAsync(
            "Recam email test",
            "Hello Stella! This is a test email from Recam.",
            "292546186xjw@gmail.com");

        return Results.Ok("Test email sent.");
    });**/
}


app.Run();
