using Microsoft.AspNetCore.Identity;
using Leagify.AuctionDrafter.Server.Data; // For ApplicationUser
using Leagify.AuctionDrafter.Shared.Models; // For Role constants

namespace Leagify.AuctionDrafter.Server
{
    public static class SeedIdentityData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Define role names from shared constants or define here
            string[] roleNames = { Role.AuctionMaster, Role.TeamCoach, Role.ProxyCoach, Role.AuctionViewer };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    // Create the roles and seed them to the database
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create a default Auction Master user
            var auctionMasterUser = await userManager.FindByEmailAsync("master@example.com");
            if (auctionMasterUser == null)
            {
                auctionMasterUser = new ApplicationUser
                {
                    UserName = "master@example.com",
                    Email = "master@example.com",
                    DisplayName = "Auction Master User",
                    EmailConfirmed = true // Typically you'd confirm email, but for dev this is easier
                };
                var createUserResult = await userManager.CreateAsync(auctionMasterUser, "MasterPassword1!"); // Use a strong password
                if (createUserResult.Succeeded)
                {
                    // Assign the AuctionMaster role to the user
                    await userManager.AddToRoleAsync(auctionMasterUser, Role.AuctionMaster);
                }
                // Log errors if createUserResult failed
            }

            // Create a default Team Coach user
            var teamCoachUser = await userManager.FindByEmailAsync("coach1@example.com");
            if (teamCoachUser == null)
            {
                teamCoachUser = new ApplicationUser
                {
                    UserName = "coach1@example.com",
                    Email = "coach1@example.com",
                    DisplayName = "Team Coach User 1",
                    EmailConfirmed = true
                };
                var createCoachResult = await userManager.CreateAsync(teamCoachUser, "CoachPassword1!");
                if (createCoachResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(teamCoachUser, Role.TeamCoach);
                }
            }
        }
    }
}
