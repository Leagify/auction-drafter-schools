using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Leagify.AuctionDrafter.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add Authorization services
builder.Services.AddAuthorizationCore();
// Add custom AuthenticationStateProvider and AuthService
builder.Services.AddScoped<Leagify.AuctionDrafter.Client.Services.PersistentAuthenticationStateProvider>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(provider => provider.GetRequiredService<Leagify.AuctionDrafter.Client.Services.PersistentAuthenticationStateProvider>());
builder.Services.AddScoped<Leagify.AuctionDrafter.Client.Services.IAuthService, Leagify.AuctionDrafter.Client.Services.AuthService>();


await builder.Build().RunAsync();
