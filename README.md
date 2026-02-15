# WWIV 3270 - WWIV BBS for 3270 Terminals

A modern .NET Core 9 recreation of the classic WWIV Bulletin Board System, specifically designed for IBM 3270 terminal emulators with additional support for standard VT/Telnet terminals.

## Project Overview

This project converts the original WWIV BBS (written in C for DOS) to VB.NET running on .NET Core 9, with dual terminal support:

- **TN3270 Protocol** (Port 2323): Full-screen forms-based interface for 3270 terminal emulators
- **Telnet/VT Protocol** (Port 23): Traditional line-based interface for standard terminal emulators

## Quick Start
1.  **Build**: `dotnet build`
2.  **Run**: `dotnet run --project wwiv3270`
3.  **Connect**: Use a 3270 emulator (like x3270 or wc3270) to connect to `localhost:2323`.
4.  **Registration**: Type `NEW` at the login prompt. **The first user to register becomes the Master Sysop (User #1, SL 255).**

## Architecture

### Core Components

- **DataStructures.vb**: VB.NET translations of C structures from `VARDEC.H` (UserRec, ConfigRec, SubBoardRec, etc.)
- **ISession**: Abstraction layer for terminal sessions
- **SessionManager**: Tracks active user sessions
- **BBS.vb**: Main server class (equivalent to `main()` in BBS.C)

### Terminal Adapters

- **TN3270SessionAdapter**: Wraps TN3270Framework sessions to implement ISession
- **TelnetSessionAdapter**: Wraps standard TCP/Telnet connections to implement ISession

### Screen System

- **IScreen**: Interface for all BBS screens
- **LoginScreen**: User authentication (supports both 3270 forms and Telnet prompts)
- **MainMenuScreen**: Main menu (replicates original WWIV menu structure)

## Building

```powershell
dotnet build
```

## Running

```powershell
dotnet run --project wwiv3270
```

The server will start two listeners:
- TN3270 on port 2323
- Telnet on port 23 (requires admin/elevated privileges on Windows)

## Connecting

### 3270 Terminal
Use a TN3270 emulator (x3270, wc3270, or similar):
```
x3270 localhost:2323
```

### Standard Terminal
Use any telnet client:
```
telnet localhost 23
```

## Current Status

### Implemented
- ✅ TN3270 and Telnet listeners
- ✅ Session management
- ✅ Login screen (3270 form-based and Telnet text-based)
- ✅ Main menu screen
- ✅ Basic user authentication (demo mode)
- ✅ Core data structures from VARDEC.H

### In Progress
- 🔄 Message base system (MSGBASE.C conversion)
- 🔄 File transfer system (XFER.C conversion)
- 🔄 User management (USER.C, NEWUSER.C conversion)
- 🔄 Configuration file loading (config.dat)

### Planned
- ⏳ Email system
- ⏳ Sub-board (message forum) system
- ⏳ File upload/download directories
- ⏳ External chains/doors support
- ⏳ Network support (WWIVnet)

## Original WWIV Source Mapping

| Original C File | VB.NET Equivalent | Status |
|----------------|-------------------|--------|
| BBS.C | BBS.vb, Screens/*.vb | Partial |
| VARDEC.H | DataStructures.vb | Complete |
| COM.C | TN3270Framework, TelnetListener | Replaced |
| USER.C | (TBD) | Planned |
| MSGBASE.C | (TBD) | Planned |
| XFER.C | (TBD) | Planned |

## Development Notes

### 3270 vs Telnet Design

The original WWIV used a character-stream model (COM port I/O). This project uses an event-driven architecture:

- **3270**: Form-based. User fills fields, presses Enter → `HandleInput` receives all field data at once
- **Telnet**: Stream-based. User types lines → async read loop processes input sequentially

The `IScreen` interface abstracts both models, with `Activate()` rendering the UI and `HandleInput()` processing user actions.

### TN3270Framework Customization

The TN3270Framework library is included in this project and may be modified as needed for WWIV-specific requirements. These modifications are project-specific and not intended for general use.

## License

This is a recreation/port of WWIV BBS. Original WWIV copyright (C) 1988-1993 by Wayne Bell.

## Credits

- Original WWIV BBS: Wayne Bell
- .NET Core Port: [Your Name]
- TN3270 Framework: Custom implementation for this project
