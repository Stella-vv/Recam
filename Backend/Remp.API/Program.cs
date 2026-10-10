using Microsoft.EntityFrameworkCore;
using Remp.DataAccess.Data;
using Remp.API.Services.Email;
using System.ComponentModel;
using Remp.API.Middlewares;
using Remp.API.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Remp.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Remp.DataAccess.Settings;
using Remp.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
JwtSettings jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? throw new InvalidOperationException("JwtSettings configuration is missing.");
if(string.IsNullOrWhiteSpace(jwtSettings.Issuer) || string.IsNullOrWhiteSpace(jwtSettings.Audience) 
                                                 || string.IsNullOrWhiteSpace(jwtSettings.SecretKey)
                                                 || jwtSettings.ExpiryMinutes <= 0)
{
    throw new InvalidOperationException("JwtSettings configuration is invalid.");
}

byte[] jwtKeyBytes = Convert.FromBase64String(jwtSettings.SecretKey);
if(jwtKeyBytes.Length < 32)
{
    throw new InvalidOperationException("JWT signing key must be at least 32 bytes.");
}

builder.Services.AddTransient<IJwtTokenService, JwtTokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => { 
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = jwtSettings.Issuer,
    ValidateAudience = true,
    ValidAudience = jwtSettings.Audience,
    ValidateLifetime = true,
    RequireExpirationTime = true,
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes),
    RequireSignedTokens = true,
    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
    NameClaimType = ClaimTypes.NameIdentifier,
    RoleClaimType = ClaimTypes.Role,
    ClockSkew = TimeSpan.Zero
};
options.MapInboundClaims = false;
});
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddSingleton<ILoginAttemptRepository, MongoLoginAttemptRepository>();
builder.Services.AddScoped<IRegisterAttemptRepository, MongoRegisterAttemptRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    string? testEmail = builder.Configuration["DevTestUser:Email"];
    string? testPassword = builder.Configuration["DevTestUser:Password"];
    string testRole = builder.Configuration["DevTestUser:Role"] ?? "user";
    if (!string.IsNullOrWhiteSpace(testEmail) && !string.IsNullOrWhiteSpace(testPassword))
    {
        using var scope = app.Services.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        ApplicationUser? testUser = await userManager.FindByEmailAsync(testEmail);

        if(testUser == null)
        {
            testUser = new ApplicationUser 
            {
                UserName = testEmail,
                Email = testEmail,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            IdentityResult createResult = await userManager.CreateAsync(testUser, testPassword);

            if (!createResult.Succeeded)
            {
                string errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException("Failed to create development test user: " + errors);
            }
        }

        bool isInRole = await userManager.IsInRoleAsync(testUser, testRole);
        if (!isInRole)
        {
            IdentityResult roleResult = await userManager.AddToRoleAsync(testUser, testRole);
            if (!roleResult.Succeeded)
            {
                string errors = string.Join("; ", roleResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException("Failed to assign development test user role: "+ errors);
            }
        }
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
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
