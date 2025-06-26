using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Leagify.AuctionDrafter.Server.Data;
using Leagify.AuctionDrafter.Server.Services; // For ICsvParsingService, IAuctionService
using Microsoft.AspNetCore.Builder; // Added for WebApplication extension methods
// No need to explicitly add 'using Leagify.AuctionDrafter.Server' for SeedIdentityData if namespace matches Program.cs implicit namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // For API controllers
builder.Services.AddRazorPages(); // For Blazor hosting support & Identity UI

// Configure DbContext for Identity using In-Memory database
builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
    options.UseInMemoryDatabase("IdentityDb")); // Name of the in-memory database

// Add ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Configure identity options here if needed (e.g., password requirements)
        options.SignIn.RequireConfirmedAccount = false; // For simplicity in dev, typically true
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 4;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<ApplicationIdentityDbContext>()
    .AddDefaultTokenProviders(); // For password reset, email confirmation tokens etc.

// Swagger/OpenAPI (optional, keep if API exploration is desired)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom application services
builder.Services.AddScoped<ICsvParsingService, CsvParsingService>();
builder.Services.AddSingleton<IAuctionService, AuctionService>(); // Singleton for in-memory auction data

var app = builder.Build();

// Seed initial Identity data (roles, default users)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Ensure ApplicationIdentityDbContext is created for in-memory
        var context = services.GetRequiredService<ApplicationIdentityDbContext>();
        context.Database.EnsureCreated();

        // Assuming SeedIdentityData is in Leagify.AuctionDrafter.Server namespace
        await Leagify.AuctionDrafter.Server.SeedIdentityData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the Identity database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
