var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // For API controllers
builder.Services.AddRazorPages(); // For Blazor hosting support

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// Only add Swagger/OpenAPI if you intend to use them for your API.
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging(); // Enable debugging for Blazor WASM
    // if (builder.Services.Any(s => s.ServiceType == typeof(SwaggerGenerator))) // Check if Swagger is registered
    // {
    //     app.UseSwagger();
    //     app.UseSwaggerUI();
    // }
}
else
{
    app.UseExceptionHandler("/Error"); // Optional: Add a generic error handler
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles(); // Serve Blazor framework files
app.UseStaticFiles(); // Serve static files from wwwroot

app.UseRouting();

app.MapRazorPages(); // Map Razor Pages
app.MapControllers(); // Map API controllers (if you have any)
app.MapFallbackToFile("index.html"); // Fallback to Blazor app for client-side routes

app.Run();
