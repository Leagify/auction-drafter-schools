# Deploying Leagify.AuctionDrafter to Azure

This document outlines the process for deploying the Leagify.AuctionDrafter application (Blazor WebAssembly frontend and ASP.NET Core backend) to Azure services. We'll primarily focus on:

*   **Azure Static Web Apps (SWA)**: For the Blazor WASM frontend and potentially a serverless API (if refactoring the backend to Azure Functions).
*   **Azure App Service**: As an alternative for hosting the ASP.NET Core backend if it remains a monolithic server application.
*   **Azure SQL Database**: For data persistence, connecting to the existing Entity Framework Core setup.
*   **Azure SignalR Service**: If real-time functionality is a core requirement (the current project structure doesn't explicitly show SignalR hubs, but this is a common addition to such apps).

## Prerequisites

*   An active Azure subscription.
*   Azure CLI installed and configured (`az login`).
*   Git installed on your local machine.
*   Source code hosted in a GitHub repository.
*   .NET SDK (matching the project's version, e.g., .NET 7.0) installed locally if deploying manually or setting up CI/CD from local.

## Deployment Strategy Options

There are two main strategies for the backend:

1.  **Backend as Azure Functions within SWA**: Refactor the ASP.NET Core API endpoints into Azure Functions. This is often the most cost-effective and streamlined approach with SWA.
2.  **Backend as Azure App Service**: Keep the ASP.NET Core backend as a separate App Service. SWA will host the Blazor WASM frontend, which will make HTTP calls to this App Service.

This guide will cover steps for **Azure Static Web Apps for the frontend** and provide considerations for both backend approaches. Azure SQL Database setup will be consistent.

## 1. Setting up Azure SQL Database

The project uses Entity Framework Core with `ApplicationIdentityDbContext.cs`, indicating a need for a relational database.

### Steps:

1.  **Create Azure SQL Database Server (if you don't have one)**:
    *   Azure Portal: Search for "SQL servers" -> "Create".
    *   Provide: Subscription, Resource Group, unique Server name, Server admin login, Password, Location.
    *   Review and Create.

2.  **Create Azure SQL Database**:
    *   Navigate to your SQL server in the Azure portal.
    *   Click "+ Create database".
    *   Provide: Database name (e.g., `LeagifyAuctionDrafterDB`), Compute + storage (choose a tier, e.g., Basic or Standard for dev/test, General Purpose Serverless can be cost-effective).
    *   **Networking**:
        *   Connectivity method: "Public endpoint".
        *   **Allow Azure services and resources to access this server**: Set to "Yes". This is crucial for SWA/App Service to connect.
        *   **Add current client IP address**: Add your IP if you need to manage the DB from your local machine (e.g., with SSMS or Azure Data Studio).
    *   Review and Create.

3.  **Get Connection String**:
    *   Go to the database's page -> "Connection strings".
    *   Copy the ADO.NET connection string. You'll replace `{your_username}` and `{your_password}`.

4.  **Configure Application**:
    *   This connection string will be set as an Application Setting/Connection String in your Azure hosting service (SWA or App Service).
    *   **Schema Migration**: You'll need to run Entity Framework Core migrations against the Azure SQL Database. This can be done:
        *   Locally, pointing to the Azure DB (ensure your IP is whitelisted).
        *   Via a CI/CD pipeline script.
        *   Using the `Update-Database` command from Package Manager Console in Visual Studio, or `dotnet ef database update` from the CLI, after configuring `appsettings.json` or user secrets to point to the Azure SQL DB.

## 2. Deploying with Azure Static Web Apps (Frontend + Optional Backend Functions)

This is ideal for Blazor WASM. SWA can build and deploy your Blazor app and optionally host an API built with Azure Functions.

### Steps:

1.  **Prepare your Application for SWA**:
    *   **API Location (if using Functions)**: If your `Leagify.AuctionDrafter.Server` project's controllers are to be refactored into Azure Functions, they should typically reside in a folder like `Api` at the root of your repository, or specified during SWA creation. For this project, the server project is `Leagify.AuctionDrafter/Server`. If you keep it as is and *don't* use SWA for the API, you'll link to an external App Service (see Option 3).
    *   **Output Location**: For a Blazor WASM project, the build output is typically in `wwwroot` relative to the *client* project's build output path, but SWA often expects it relative to the solution or a specific client project folder. When `Leagify.AuctionDrafter.Server` builds, it places the client's static assets in its own `wwwroot` folder. The `Leagify.AuctionDrafter.Client` project's output path after `dotnet publish` is usually `bin/Release/netX.X/publish/wwwroot`. You'll need to confirm the exact path SWA expects. Often, for a hosted Blazor WASM app, the `App location` is the path to the Server project, and SWA handles finding the client assets.

2.  **Create Azure Static Web App in Azure Portal**:
    *   Search for "Static Web App" -> "Create".
    *   **Basics**:
        *   Subscription, Resource Group, Name, Region.
        *   **Hosting plan**: Standard (or Free for hobby projects).
        *   **Source**: GitHub. Sign in and select your Organization, Repository (`Leagify.AuctionDrafter`), and Branch (e.g., `main`).
    *   **Build Details**:
        *   **Build Presets**: Select "Blazor".
        *   **App location**: Path to your Blazor application code. For a hosted Blazor WASM app like this, this is typically the path to the **Server** project: `Leagify.AuctionDrafter/Server`.
        *   **Api location**:
            *   If you refactored your backend to Azure Functions within the repo (e.g., in an `Api` folder): `Api`.
            *   If your backend is the existing `Leagify.AuctionDrafter.Server` project and you want SWA to try and build it as functions (less common for full ASP.NET Core apps): `Leagify.AuctionDrafter/Server`.
            *   **If using a separate App Service for the backend (see Section 3), leave this blank.**
        *   **Output location**: For Blazor, this is often relative to the "App location". When the Server project builds, it serves the client from its `wwwroot`. SWA usually figures this out for Blazor preset, but it might require tweaking. Common values are `wwwroot` (if App location is Server project) or `build` or `dist`. The Blazor preset default is usually correct.
    *   Review + create. This creates a GitHub Actions workflow in your repository.

3.  **Configure Application Settings (including DB Connection String)**:
    *   Once SWA is created, go to its "Configuration" section.
    *   Add an "Application setting" for your SQL Database connection string. Name it appropriately (e.g., `DefaultConnection` if that's what `appsettings.json` uses, or how EF Core is configured in `Program.cs` of the Server project).
    *   If your API is part of SWA (Azure Functions), any other required app settings go here.

4.  **GitHub Actions Workflow**:
    *   The workflow file (e.g., `.github/workflows/azure-static-web-apps-....yml`) will be added to your repo.
    *   It will build and deploy your Blazor WASM app. If an API location was specified, it attempts to deploy that too.
    *   Monitor the "Actions" tab in GitHub.

5.  **Accessing the App**:
    *   The SWA overview page in Azure will show the URL.

## 3. Deploying ASP.NET Core Backend to Azure App Service (If not using SWA for API)

If `Leagify.AuctionDrafter.Server` remains a full ASP.NET Core application, deploy it to Azure App Service.

### Steps:

1.  **Create Azure App Service**:
    *   Azure Portal: Search for "App Services" -> "Create".
    *   **Basics**:
        *   Subscription, Resource Group.
        *   **Name**: Unique name for your app service (e.g., `leagify-auctiondrafter-api`).
        *   **Publish**: Code.
        *   **Runtime stack**: .NET 8 (or your project's version).
        *   **Operating System**: Linux (often more cost-effective) or Windows.
        *   **Region**.
    *   **App Service Plan**: Create new or use existing. Choose a pricing tier.
    *   Review + create.

2.  **Deployment**:
    *   **CI/CD (Recommended)**: Use GitHub Actions, Azure DevOps, or App Service Deployment Center to set up continuous deployment from your repository.
        *   For GitHub Actions, you can create a workflow that builds `Leagify.AuctionDrafter.Server` and deploys it using `azure/webapps-deploy@v2`.
    *   **Manual Publish**: From Visual Studio (right-click Server project -> Publish) or using `dotnet publish` and then uploading via FTP/ZipDeploy.

3.  **Configuration**:
    *   In the App Service -> "Configuration" -> "Application settings":
        *   Add the SQL Database connection string (mark it as a "SQLAzure" type if desired, or just a regular app setting).
        *   Set `ASPNETCORE_ENVIRONMENT` to `Production`.
        *   Any other required settings from `appsettings.json`.
    *   **CORS**: If your Blazor WASM app (on SWA) calls this App Service API, you **must** configure CORS in the App Service:
        *   Go to App Service -> "API" -> "CORS".
        *   Add the URL of your Static Web App (e.g., `https://<your-swa-name>.azurestaticapps.net`) to the "Allowed Origins".

4.  **Linking SWA to this App Service API (if SWA hosts only frontend)**:
    *   This is more of an application-level configuration. Your Blazor WASM client's HTTP requests (using `HttpClient`) need to be configured to point to the URL of this Azure App Service (e.g., `https://leagify-auctiondrafter-api.azurewebsites.net`). This base address can be configured in `Program.cs` of the Client project, potentially read from SWA's application settings.

## 4. Setting up Azure SignalR Service (Optional - If Adding Real-Time)

If you add SignalR functionality:

1.  **Create Azure SignalR Service Instance**:
    *   Azure Portal: "Create a resource" -> "SignalR Service".
    *   Provide: Subscription, Resource Group, Name, Region, Pricing Tier (Free or Standard).
    *   **Service Mode**:
        *   "Serverless": If your backend SignalR logic is in Azure Functions (common with SWA).
        *   "Default" or "Classic": If your ASP.NET Core App Service hosts the SignalR hubs.
    *   Review + create.

2.  **Get Connection String**:
    *   SignalR Service resource -> "Keys" -> Copy Primary Connection String.

3.  **Integrate with Application**:
    *   **Backend (App Service or Functions)**:
        *   Add NuGet package: `Microsoft.Azure.SignalR`.
        *   In `Program.cs` (or `Startup.cs`):
            ```csharp
            // Example for ASP.NET Core App Service
            builder.Services.AddSignalR().AddAzureSignalR(options =>
            {
                options.ConnectionString = builder.Configuration.GetConnectionString("AzureSignalR");
            });
            // ...
            app.MapHub<MyChatHub>("/mychathub"); // Example hub
            ```
        *   Add the `AzureSignalR` connection string to App Service Configuration / SWA Application Settings.
    *   **Frontend (Blazor WASM Client)**:
        *   The client will connect to the SignalR endpoint. This might be directly to the Azure SignalR service endpoint (in Serverless mode, often negotiated via a Function) or to your App Service endpoint (which then uses the Azure SignalR backplane).

## Final Configuration and DNS

*   **Custom Domains**: Configure custom domains for your SWA and/or App Service if needed.
*   **HTTPS**: Azure services provide managed certificates, ensure HTTPS is enforced.
*   **Monitoring**: Use Azure Monitor and Application Insights for logging and performance monitoring.

This guide provides a comprehensive overview. Adapt the steps based on whether you choose to refactor the backend to Azure Functions for SWA or deploy it as a separate App Service. Always consult the latest Azure documentation for specific service features and configurations.
