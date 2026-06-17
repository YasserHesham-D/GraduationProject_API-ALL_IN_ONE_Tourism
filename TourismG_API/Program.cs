using Domain.Models;
using Infrastructure.DbContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Presentation.ServiceExtensions;
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

builder.Services.AddControllers();

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
                     
                      "- **Customers:** customer1@example.com to customer4@example.com (Password: `Customer@123456`)"
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

// ✅ Correct order
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

app.MapControllers();



// using (var scope = app.Services.CreateScope())
// {
//     var seedService = scope.ServiceProvider.GetRequiredService<SeedDataService>();
//     await seedService.InitializeAsync();
// }

app.Run();