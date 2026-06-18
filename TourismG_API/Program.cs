using Domain.Models;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Presentation.ServiceExtensions;
using Presentation.Services;
//using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HostConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ServicesCollection();

builder.Services.AddScoped<IFileUploadService, FileUploadService>();

builder.Services.AddControllers();

// Add SignalR
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TourismG API",
        Version = "v1",
        Description = "API documentation for the TourismG mobile tourism app.\n\n" +
                      "**Seeded Test Credentials:**\n" +
                      "- **Admin:** admin@example.com (Password: `Admin@123456`)\n\n" +
                     
                      "- **Providers:** provider1@example.com to provider4@example.com (Password: `Provider@123456`)\n\n" +
                     
                      "- **Customers:** customer1@example.com to customer4@example.com (Password: `Customer@123456`)\n\n" +

                      "- **Guideds:** khaled.guide@tourism.eg , amr.guide@tourism.eg , marian.guide@tourism.eg , joe.guide@tourism.eg  (Password: `Guide@123456`)\n\n"

    });

options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token only. Swagger will add the Bearer prefix."
    });

    // ✅ Swashbuckle 10.x + Microsoft.OpenApi 2.x correct pattern
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.CustomJwtAuth(builder.Configuration);

builder.Services.AddAuthorization();

var app = builder.Build();

// Static files middleware for serving uploaded files
app.UseStaticFiles();

// CORS Middleware
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TourismG API v1");
    options.RoutePrefix = "swagger";
});
app.MapGet("/", () => Results.Redirect("/swagger"));

// File serving endpoint
app.MapGet("/api/files/{*filePath}", async (string filePath, IWebHostEnvironment env) =>
{
    try
    {
        var combinedPath = Path.Combine(env.WebRootPath ?? "wwwroot", filePath ?? string.Empty);

        // Security: prevent path traversal
        var fullPath_normalized = Path.GetFullPath(combinedPath);
        var webroot_normalized = Path.GetFullPath(env.WebRootPath ?? "wwwroot");
        if (!fullPath_normalized.StartsWith(webroot_normalized, StringComparison.OrdinalIgnoreCase))
        {
            return Results.Forbid();
        }

        if (!System.IO.File.Exists(fullPath_normalized))
            return Results.NotFound();

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath_normalized);
        var contentType = GetContentType(fullPath_normalized);
        return Results.File(bytes, contentType);
    }
    catch
    {
        return Results.NotFound();
    }
});

// Map controllers
app.MapControllers();

// Map SignalR hubs
app.MapHub<Presentation.Hubs.ChatHub>("/hubs/chat");

//using (var scope = app.Services.CreateScope())
//{
//    var seedService = scope.ServiceProvider.GetRequiredService<SeedDataService>();
//    await seedService.InitializeAsync();
//}

app.Run();

// Helper method to determine content type
string GetContentType(string filePath)
{
    var ext = Path.GetExtension(filePath).ToLowerInvariant();
    return ext switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".bmp" => "image/bmp",
        ".tiff" => "image/tiff",
        ".ico" => "image/x-icon",
        _ => "application/octet-stream"
    };
}
