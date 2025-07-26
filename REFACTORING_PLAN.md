# Refactoring Plan: ASP.NET Core Backend to Azure Functions

This document outlines the step-by-step plan for refactoring the existing ASP.NET Core backend to a serverless backend using Azure Functions. This change will align the project with the recommended architecture for Azure Static Web Apps, leading to a more streamlined and cost-effective deployment.

## Phase 1: Project Setup and Initial Function Creation

1.  **Create a new Azure Functions project:**
    *   Create a new C# Azure Functions project named `Leagify.AuctionDrafter.Api` within the existing solution.
    *   The project will use the .NET 8 runtime and the isolated worker model for compatibility with the latest features.

2.  **Add project references:**
    *   Add a project reference from the new `Leagify.AuctionDrafter.Api` project to the `Leagify.AuctionDrafter.Shared` project to access the shared models and DTOs.

3.  **Create the first Azure Function:**
    *   Create a new Azure Function named `CreateAuction` within the `Leagify.AuctionDrafter.Api` project.
    *   This function will be responsible for creating a new auction and will be triggered by an HTTP request.
    *   The initial implementation will be a simple "hello world" style function to verify that the project is set up correctly.

4.  **Update the solution file:**
    *   Add the new `Leagify.AuctionDrafter.Api` project to the `Leagify.AuctionDrafter.sln` solution file.

## Phase 2: Migrating the API Logic

1.  **Migrate the `CreateAuction` logic:**
    *   Move the logic for creating a new auction from the `AuctionController` in the `Leagify.AuctionDrafter.Server` project to the `CreateAuction` function in the `Leagify.AuctionDrafter.Api` project.
    *   This will involve adapting the code to the Azure Functions programming model, including using the correct bindings for HTTP triggers and responses.

2.  **Migrate the `GetAuction` logic:**
    *   Create a new Azure Function named `GetAuction`.
    *   Move the logic for retrieving an auction by its ID from the `AuctionController` to the `GetAuction` function.

3.  **Migrate the `GetAuctionSchools` logic:**
    *   Create a new Azure Function named `GetAuctionSchools`.
    *   Move the logic for retrieving the schools for a given auction from the `AuctionController` to the `GetAuctionSchools` function.

4.  **Migrate the `AssignRole` logic:**
    *   Create a new Azure Function named `AssignRole`.
    *   Move the logic for assigning a role to a user from the `AuctionController` to the `AssignRole` function.

## Phase 3: Updating the Client Application

1.  **Update `CreateAuctionPage.razor`:**
    *   Update the `CreateAuctionPage.razor` component to call the new `CreateAuction` Azure Function instead of the old API endpoint.

2.  **Update `AuctionWaitingRoom.razor`:**
    *   Update the `AuctionWaitingRoom.razor` component to call the new `GetAuction` and `AssignRole` Azure Functions.

3.  **Update `AuctionDetailsPage.razor`:**
    *   Update the `AuctionDetailsPage.razor` component to call the new `GetAuction` and `GetAuctionSchools` Azure Functions.

## Phase 4: Updating the Build and Deployment Process

1.  **Update the GitHub Actions workflow:**
    *   Update the `azure-static-web-apps.yml` workflow to build and deploy the new `Leagify.AuctionDrafter.Api` project along with the Blazor client.
    *   This will involve changing the `api_location` to point to the new Azure Functions project.

2.  **Update the `tasks.json` file:**
    *   Update the `.vscode/tasks.json` file to build the new `Leagify.AuctionDrafter.Api` project instead of the old `Leagify.AuctionDrafter.Server` project.

## Phase 5: Cleanup

1.  **Remove the old `Leagify.AuctionDrafter.Server` project:**
    *   Once all the API logic has been migrated to Azure Functions and the client application has been updated to use the new functions, the old `Leagify.AuctionDrafter.Server` project can be removed from the solution.

2.  **Update the solution file:**
    *   Remove the `Leagify.AuctionDrafter.Server` project from the `Leagify.AuctionDrafter.sln` solution file.

By following this plan, we can systematically refactor the backend to use Azure Functions, ensuring that the application remains functional throughout the process.
