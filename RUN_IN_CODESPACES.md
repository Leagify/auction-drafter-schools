# Running Leagify.AuctionDrafter in GitHub Codespaces

This document describes how to set up, run, and debug the Leagify.AuctionDrafter application within GitHub Codespaces. This project is a Blazor WebAssembly application with an ASP.NET Core backend.

## Setup

1.  **Open in Codespaces**:
    *   Navigate to the GitHub repository for Leagify.AuctionDrafter.
    *   Click on the "Code" button.
    *   Select "Open with Codespaces" and then "New codespace".
    *   GitHub Codespaces will automatically set up the environment based on the `.devcontainer/devcontainer.json` file. This includes pulling the necessary Docker image, installing .NET SDK, Azure CLI, PowerShell, and VS Code extensions.
    *   The `postCreateCommand` will automatically run `dotnet restore Leagify.AuctionDrafter/Leagify.AuctionDrafter.sln` to install all project dependencies.

2.  **Verify Dependencies (Optional)**:
    *   Once the codespace is ready, you can open a terminal (Ctrl+` or Cmd+`) and verify the .NET SDK version:
        ```bash
        dotnet --version
        ```
    *   You can also manually restore dependencies if needed:
        ```bash
        dotnet restore Leagify.AuctionDrafter/Leagify.AuctionDrafter.sln
        ```

## Running the Application

The application consists of a server project (`Leagify.AuctionDrafter.Server`) which hosts the Blazor WASM client.

1.  **Navigate to the Server Project Directory**:
    *   In the VS Code terminal within Codespaces:
        ```bash
        cd Leagify.AuctionDrafter/Server
        ```

2.  **Run the Server Project**:
    *   Use the following command to build and run the server:
        ```bash
        dotnet run
        ```
    *   Alternatively, you can use the launch configuration provided (see Debugging section).

3.  **Port Forwarding & Accessing the App**:
    *   GitHub Codespaces will automatically detect that the application is listening on ports 5001 (HTTPS) and 5000 (HTTP) as configured in `Leagify.AuctionDrafter/Server/Properties/launchSettings.json` and `.devcontainer/devcontainer.json`.
    *   A notification should appear prompting you to "Open in Browser" for port 5001 (Backend HTTPS). Click this to open the application in a new tab.
    *   If the notification doesn't appear, go to the "Ports" tab in the VS Code sidebar. You should see `5001` and `5000` listed. Click the globe icon next to port `5001` to open it in a browser.
    *   The application will be accessible at a URL similar to `https://[codespacename]-[port].preview.app.github.dev/`.

## Debugging

The `.devcontainer/devcontainer.json` pre-installs the C# extension, which provides debugging capabilities.

### Debugging the Server (ASP.NET Core Backend) and Client (Blazor WASM)

A default launch configuration is usually sufficient for Blazor WASM projects hosted by an ASP.NET Core backend.

1.  **Ensure `.vscode/launch.json` is configured (usually automatic)**:
    *   When you open the "Run and Debug" view (Ctrl+Shift+D or Cmd+Shift+D) for the first time with a .NET project, VS Code might prompt you to add required assets to build and debug. Click "Yes". This will create a `.vscode/launch.json` and `tasks.json` if they don't exist.
    *   A typical `launch.json` for running the server (which also serves the client) would look like this:

    ```json
    // .vscode/launch.json
    {
        "version": "0.2.0",
        "configurations": [
            {
                "name": ".NET Core Launch (web)",
                "type": "coreclr",
                "request": "launch",
                "preLaunchTask": "build",
                "program": "${workspaceFolder}/Leagify.AuctionDrafter/Server/bin/Debug/net8.0/Leagify.AuctionDrafter.Server.dll",
                "args": [],
                "cwd": "${workspaceFolder}/Leagify.AuctionDrafter/Server",
                "stopAtEntry": false,
                "serverReadyAction": {
                    "action": "openExternally",
                    "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
                },
                "env": {
                    "ASPNETCORE_ENVIRONMENT": "Development"
                },
                "sourceFileMap": {
                    "/Views": "${workspaceFolder}/Views"
                }
            },
            {
                "name": ".NET Core Attach",
                "type": "coreclr",
                "request": "attach"
            }
        ]
    }
    ```
    *   **Note**: The `program` path might vary slightly based on your exact .NET version and build configuration. Ensure the `preLaunchTask: "build"` corresponds to a task in `tasks.json` that builds the `Leagify.AuctionDrafter.Server` project or the entire solution.

2.  **Set Breakpoints**:
    *   **Server-side C# code**: Open any `.cs` file in the `Leagify.AuctionDrafter.Server` or `Leagify.AuctionDrafter.Shared` projects and click in the gutter to the left of the line numbers.
    *   **Client-side C# code (Blazor WASM)**: Open any `.razor` or `.cs` file in the `Leagify.AuctionDrafter.Client` project and set breakpoints. Debugging for Blazor WASM happens in the browser's developer tools, but VS Code can connect to it.

3.  **Start Debugging**:
    *   Go to the "Run and Debug" view (Ctrl+Shift+D or Cmd+Shift+D).
    *   Select the ".NET Core Launch (web)" configuration from the dropdown.
    *   Click the green "Start Debugging" play button or press F5.
    *   The server will build and start. Execution will pause at any server-side breakpoints.
    *   For client-side Blazor WASM debugging:
        *   Once the app is running and open in your browser (preferably Chrome or Edge), you might need to enable debugging for WebAssembly.
        *   In VS Code, you can often start a "Debug Blazor WebAssembly in Browser" session if the Blazor WASM Companion extension is active and correctly configured, or use the browser's DevTools (Shift+Alt+D in Edge/Chrome on Windows when the app tab is active).
        *   Breakpoints set in `.razor` files or client-side C# files should be hit when the code executes in the browser.

4.  **Debugging Tools**:
    *   Use the debug toolbar in VS Code to step through code (continue, step over, step into, step out), inspect variables, and watch expressions for server-side code.
    *   Use the browser's developer tools for similar functionality for client-side Blazor WASM code.

## Common Issues

*   **Port Conflicts**: If the application fails to start (e.g., ports 5000/5001 are in use by another process), ensure no other services are running on these ports. The "Ports" tab in Codespaces can help manage forwarded ports.
*   **Build Errors**: If `dotnet run` or the build task fails, check the "PROBLEMS" tab in VS Code for error details. Common causes include syntax errors or missing dependencies (though `dotnet restore` should handle the latter).
*   **HTTPS Certificate**: Codespaces provides a default certificate. If you encounter HTTPS trust issues, you might need to tell your browser to proceed. For local development outside Codespaces, you'd use `dotnet dev-certs https --trust`.
*   **Blazor WASM Debugging Not Working**:
    *   Ensure the "JS Debugger" extension is enabled in VS Code (usually by default).
    *   Ensure the Blazor WASM Companion extension is installed.
    *   Make sure you're using a compatible browser (latest Chrome or Edge).
    *   Check the browser's console for any error messages.

For more detailed information on .NET development and debugging in VS Code, refer to the [official Microsoft documentation](https://code.visualstudio.com/docs/languages/csharp) and [Blazor documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/debug).
