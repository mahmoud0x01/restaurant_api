// Program.cs
using Mahmoud_Restaurant.Configurations;
using Mahmoud_Restaurant.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Collections.Generic;
using System.Linq;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
builder.Services.AddScoped<AuthService>((serviceProvider) =>
{
    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
    var jwtSecret = builder.Configuration.GetValue<string>("JwtSettings:Secret");
    var adminSecretKey = builder.Configuration.GetValue<string>("Admin:Secret");
    return new AuthService(context, jwtSecret, adminSecretKey);
});

builder.Services.AddOpenApiDocument(config =>
{
    config.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token"
    });

    // Use the updated custom processor to apply security only where required
    config.OperationProcessors.Add(new AuthorizeSecurityProcessor());
});

// Use NSwag middleware to serve Swagger UI
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseOpenApi();       // Enable serving OpenAPI JSON
app.UseSwaggerUi();    // Enable Swagger UI

app.MapControllers();

app.Run();
