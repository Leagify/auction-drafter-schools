using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Leagify.AuctionDrafter.Server.Data;
using Leagify.AuctionDrafter.Server.Services; // For ICsvParsingService, IAuctionService
using Microsoft.AspNetCore.Builder; // Added for WebApplication extension methods
// No need to explicitly add 'using Leagify.AuctionDrafter.Server' for SeedIdentityData if namespace matches Program.cs implicit namespace

var builder = WebApplication.CreateBuilder(args);

var DefaultCorsPolicy = "_defaultCorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: DefaultCorsPolicy,
                      policy =>
                      {
                          // For development, allowing the app's own base address.
                          // In production, you might list specific domains or be more restrictive.
                          // Using builder.HostEnvironment.BaseAddress might be problematic if it's not what the browser perceives as the origin.
                          // For Codespaces, the forwarded public URL is the client origin.
                          // Allowing any origin with credentials is a security risk for production.
                          // SetIsOriginAllowed(_ => true) allows any origin but is compatible with AllowCredentials.
                          policy.SetIsOriginAllowed(_ => true) // Allows any origin, for dev/testing with credentials
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                      });
});

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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
});

// Swagger/OpenAPI (optional, keep if API exploration is desired)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom application services
builder.Services.AddSingleton<ICsvParsingService, CsvParsingService>(); // Changed from Scoped to Singleton
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

// Apply the CORS policy. IMPORTANT: This should generally be before UseRouting, UseAuthentication, UseAuthorization.
app.UseCors(DefaultCorsPolicy);

app.UseRouting();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
