# Visual Studio 2026 Setup Guide

## Opening the Project in Visual Studio 2026

### Method 1: Open Solution File
1. Launch **Visual Studio 2026**
2. Click **File** → **Open** → **Project/Solution**
3. Navigate to the project directory
4. Select `wwiv3270.sln`
5. Click **Open**

### Method 2: Clone from GitHub
1. Launch **Visual Studio 2026**
2. Click **Clone a repository**
3. Enter repository URL: `https://github.com/SComps/wwiv_dotnet.git`
4. Choose local path
5. Click **Clone**
6. Visual Studio will automatically open the solution

## Project Structure in Solution Explorer

```
Solution 'wwiv3270' (2 of 2 projects)
├── wwiv3270 (Startup Project)
│   ├── Dependencies
│   │   └── Projects
│   │       └── TN3270Framework
│   ├── Interfaces
│   │   └── IScreen.vb
│   ├── Screens
│   │   ├── LoginScreen.vb
│   │   └── MainMenuScreen.vb
│   ├── Telnet
│   │   ├── TelnetListener.vb
│   │   └── TelnetSessionAdapter.vb
│   ├── BBS.vb
│   ├── DataStructures.vb
│   ├── ISession.vb
│   ├── Program.vb
│   ├── SessionManager.vb
│   └── TN3270SessionAdapter.vb
└── TN3270Framework (Library)
    └── TN3270Listener.vb
```

## Building the Project

### Debug Build
1. Select **Debug** configuration from toolbar dropdown
2. Press **F6** or click **Build** → **Build Solution**
3. Output will be in `wwiv3270\bin\Debug\net9.0\`

### Release Build
1. Select **Release** configuration from toolbar dropdown
2. Press **F6** or click **Build** → **Build Solution**
3. Output will be in `wwiv3270\bin\Release\net9.0\`

## Running the Project

### Start Debugging (F5)
1. Ensure `wwiv3270` is set as the startup project (right-click → **Set as Startup Project**)
2. Press **F5** or click **Debug** → **Start Debugging**
3. Console window will open showing:
   ```
   Starting WWIV BBS Server...
   TN3270 Listener started on port 2323
   Telnet Listener started on port 23
   Server is running. Press Ctrl+C to stop.
   ```

### Start Without Debugging (Ctrl+F5)
1. Press **Ctrl+F5** or click **Debug** → **Start Without Debugging**
2. Same as above, but debugger won't attach

### Running as Administrator (Required for Port 23)
Port 23 (Telnet) requires elevated privileges on Windows:
1. Close Visual Studio
2. Right-click Visual Studio 2026 icon
3. Select **Run as administrator**
4. Open the solution
5. Run normally (F5 or Ctrl+F5)

**Alternative**: Change Telnet port to 2323 in `BBS.vb` if you don't need standard port 23.

## Debugging

### Breakpoints
1. Click in the left margin of any code line to set a breakpoint (red dot)
2. Press **F5** to start debugging
3. Execution will pause at breakpoints
4. Use **F10** (Step Over), **F11** (Step Into), **Shift+F11** (Step Out)

### Useful Breakpoint Locations
- `BBS.vb` → `OnTNConnectionReceived` - When 3270 client connects
- `LoginScreen.vb` → `HandleTN3270Input` - When user submits login
- `TN3270SessionAdapter.vb` → `OnAidKey` - When any 3270 key is pressed

### Watch Windows
1. While debugging, go to **Debug** → **Windows** → **Watch** → **Watch 1**
2. Add variables to watch (e.g., `session.User.Name`, `e.AidKey`)

### Immediate Window
1. While debugging, press **Ctrl+Alt+I** or go to **Debug** → **Windows** → **Immediate**
2. Type VB.NET expressions to evaluate:
   ```vb
   ? session.SessionId
   ? session.User.Name
   ```

## IntelliSense and Code Navigation

### Go to Definition
- **F12** on any symbol to jump to its definition
- **Ctrl+Click** also works

### Find All References
- **Shift+F12** to find all uses of a symbol

### Peek Definition
- **Alt+F12** to view definition in a popup without navigating away

### Code Snippets
- Type `Sub` and press **Tab** to insert a subroutine template
- Type `Function` and press **Tab** to insert a function template
- Type `Class` and press **Tab** to insert a class template

## NuGet Package Management

### View Installed Packages
1. Right-click on project in Solution Explorer
2. Select **Manage NuGet Packages**
3. Click **Installed** tab

### Add New Package
1. Right-click on project
2. Select **Manage NuGet Packages**
3. Click **Browse** tab
4. Search for package (e.g., "zmodem")
5. Click **Install**

## Testing Connections

### Test TN3270 Connection
1. Start the project (F5)
2. Open a 3270 emulator:
   - **x3270** (Linux): `x3270 localhost:2323`
   - **wc3270** (Windows): `wc3270 localhost:2323`
3. You should see the login screen

### Test Telnet Connection
1. Start the project **as Administrator** (F5)
2. Open a terminal:
   - **Windows**: `telnet localhost 23`
   - **PuTTY**: Connect to `localhost` port `23`, protocol `Telnet`
3. You should see the login screen

## Common Issues

### Issue: "Port already in use"
**Solution**: Another process is using port 2323 or 23
```powershell
# Find process using port
netstat -ano | findstr :2323
netstat -ano | findstr :23

# Kill process (replace PID with actual process ID)
taskkill /PID <PID> /F
```

### Issue: "Access denied" on port 23
**Solution**: Run Visual Studio as Administrator (see above)

### Issue: Build fails with "Type not found"
**Solution**: 
1. Clean solution: **Build** → **Clean Solution**
2. Rebuild: **Build** → **Rebuild Solution**

### Issue: Changes not taking effect
**Solution**: 
1. Stop debugging
2. Clean solution
3. Rebuild solution
4. Start debugging again

## Recommended Extensions for Visual Studio 2026

### VB.NET Development
- **Visual Basic Productivity Power Tools** - Enhanced IntelliSense and refactoring
- **CodeMaid** - Code cleanup and organization
- **ReSharper** (optional, commercial) - Advanced code analysis

### General Development
- **GitHub Extension** - Integrated GitHub support
- **Markdown Editor** - Edit README and documentation files
- **Output Enhancer** - Colorized build output

## Project Settings

### Target Framework
- **.NET Core 9.0** (net9.0)
- Configured in `wwiv3270.vbproj`

### Language Version
- **Visual Basic 16.9** (latest)
- Supports all modern VB.NET features

### Output Type
- **Console Application** (Exe)
- Runs in a console window

## Git Integration in Visual Studio

### Commit Changes
1. Go to **View** → **Git Changes**
2. Review changed files
3. Enter commit message
4. Click **Commit All**

### Push to GitHub
1. After committing, click **Push** button
2. Changes will be pushed to `https://github.com/SComps/wwiv_dotnet.git`

### Pull Latest Changes
1. Go to **View** → **Git Changes**
2. Click **Pull** button

### Create Branch
1. Go to **View** → **Git Changes**
2. Click branch dropdown at bottom
3. Click **New Branch**
4. Enter branch name (e.g., `feature/message-system`)
5. Click **Create**

## Performance Profiling

### CPU Profiling
1. Go to **Debug** → **Performance Profiler**
2. Select **CPU Usage**
3. Click **Start**
4. Use the application
5. Click **Stop Collection**
6. Analyze results

### Memory Profiling
1. Go to **Debug** → **Performance Profiler**
2. Select **.NET Object Allocation Tracking**
3. Click **Start**
4. Use the application
5. Click **Stop Collection**
6. Analyze memory allocations

## Publishing for Deployment

### Self-Contained Deployment
```powershell
dotnet publish -c Release -r win-x64 --self-contained
```
Output: `wwiv3270\bin\Release\net9.0\win-x64\publish\`

### Framework-Dependent Deployment
```powershell
dotnet publish -c Release
```
Output: `wwiv3270\bin\Release\net9.0\publish\`

## Next Steps

1. **Explore the code**: Start with `Program.vb` and `BBS.vb`
2. **Set breakpoints**: Debug the login flow
3. **Test connections**: Connect with 3270 and Telnet clients
4. **Review documentation**: Read `README.md`, `ROADMAP.md`, and `CONVERSION_STATUS.md`
5. **Start implementing**: Pick a feature from `ROADMAP.md` and begin coding!

## Support

- **GitHub Issues**: https://github.com/SComps/wwiv_dotnet/issues
- **Documentation**: See `docs/` folder
- **Original WWIV**: Reference `e:\wwiv_source\` for C source code

## Quick Reference

| Action | Shortcut |
|--------|----------|
| Build Solution | F6 |
| Start Debugging | F5 |
| Start Without Debugging | Ctrl+F5 |
| Stop Debugging | Shift+F5 |
| Step Over | F10 |
| Step Into | F11 |
| Toggle Breakpoint | F9 |
| Go to Definition | F12 |
| Find All References | Shift+F12 |
| Comment Selection | Ctrl+K, Ctrl+C |
| Uncomment Selection | Ctrl+K, Ctrl+U |
| Format Document | Ctrl+K, Ctrl+D |
| IntelliSense | Ctrl+Space |
