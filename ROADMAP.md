# WWIV 3270 Development Roadmap

## Phase 1: Foundation (COMPLETED ✅)
- [x] Project structure and build system
- [x] Core data structures from VARDEC.H
- [x] Session management framework
- [x] TN3270 and Telnet listeners
- [x] Screen abstraction layer
- [x] Login screen (both terminal types)
- [x] Main menu screen (both terminal types)

## Phase 2: User System (NEXT)

### 2.1 User File I/O
**Files to Convert**: USER.C, parts of BBS.C
**New Files**: `Services/UserService.vb`, `IO/UserFileReader.vb`

Tasks:
- [ ] Implement binary reader for USER.LST
- [ ] Implement binary writer for USER.LST
- [ ] Create user lookup by name (NAMES.LST equivalent)
- [ ] Implement user record caching
- [ ] Add user save/update functionality

**Original C Functions to Port**:
- `read_user(int un, userrec *u)` → `UserService.LoadUser(userNum)`
- `write_user(int un, userrec *u)` → `UserService.SaveUser(userNum, user)`
- `finduser(char *s)` → `UserService.FindUserByName(name)`

### 2.2 New User Creation
**Files to Convert**: NEWUSER.C
**New Files**: `Screens/NewUserScreen.vb`

Tasks:
- [ ] Create new user registration screen (3270)
- [ ] Create new user registration flow (Telnet)
- [ ] Implement validation rules
- [ ] Auto-validation support
- [ ] New user questionnaire

### 2.3 User Editor
**Files to Convert**: Parts of BBS.C (user defaults)
**New Files**: `Screens/UserSettingsScreen.vb`

Tasks:
- [ ] User settings screen
- [ ] Password change
- [ ] Profile editing
- [ ] Color/display preferences

## Phase 3: Configuration System

### 3.1 Config File Loading
**Files to Convert**: BBS.C (initialization)
**New Files**: `Services/ConfigService.vb`, `IO/ConfigFileReader.vb`

Tasks:
- [ ] Binary reader for CONFIG.DAT
- [ ] Load system configuration on startup
- [ ] Load sub-board definitions (SUBS.DAT)
- [ ] Load directory definitions (DIRS.DAT)
- [ ] Load security level definitions

### 3.2 System Status
**Files to Convert**: Parts of BBS.C, UTILITY.C
**New Files**: `Services/StatusService.vb`

Tasks:
- [ ] STATUS.DAT reading/writing
- [ ] Caller number tracking
- [ ] Daily statistics
- [ ] System date tracking

## Phase 4: Message System

### 4.1 Message Base Core
**Files to Convert**: MSGBASE.C, MSGBASE1.C
**New Files**: `Services/MessageService.vb`, `Models/Message.vb`

Tasks:
- [ ] Message storage abstraction
- [ ] Read messages from *.SUB files
- [ ] Write messages to *.SUB files
- [ ] Message header parsing
- [ ] Message text reading (various storage types)

**Key Original Functions**:
- `open_sub(int b)` → `MessageService.OpenSubBoard(subNum)`
- `read_post(int mn, postrec *p)` → `MessageService.ReadPost(msgNum)`
- `post()` → `MessageService.CreatePost()`

### 4.2 Message Reading
**New Files**: `Screens/MessageReadScreen.vb`, `Screens/MessageListScreen.vb`

Tasks:
- [ ] Message list display (3270 table, Telnet list)
- [ ] Message reader with threading
- [ ] Next/Previous navigation
- [ ] Reply functionality
- [ ] Quote text handling

### 4.3 Message Posting
**New Files**: `Screens/MessagePostScreen.vb`, `Services/EditorService.vb`

Tasks:
- [ ] Message editor (line-based for both terminals)
- [ ] Title/subject input
- [ ] Anonymous posting support
- [ ] Message validation
- [ ] Post to sub-board

### 4.4 Message Scanning
**Files to Convert**: Parts of BBS.C (qscan)

Tasks:
- [ ] Quick-scan implementation
- [ ] New message detection
- [ ] Scan pointer management

## Phase 5: File Transfer System

### 5.1 File Directories
**Files to Convert**: XFER.C, XFEROVL.C, XFEROVL1.C
**New Files**: `Services/FileService.vb`, `Screens/FileListScreen.vb`

Tasks:
- [ ] Read upload directory definitions
- [ ] List files in directory
- [ ] File search functionality
- [ ] Extended descriptions
- [ ] Batch download queue

### 5.2 IND$FILE Implementation (3270)
**New Files**: `Services/Protocols/IndFileProtocol.vb`

**Priority**: HIGH (easiest file transfer to implement)

Tasks:
- [ ] Implement IND$FILE PUT (send to terminal)
  - [ ] Send structured field command
  - [ ] Stream file data in chunks
  - [ ] Send EOF marker
  - [ ] Handle errors
- [ ] Implement IND$FILE GET (receive from terminal)
  - [ ] Send GET command
  - [ ] Receive structured field data
  - [ ] Write to temporary file
  - [ ] Validate and move to final location
- [ ] Add progress display on 3270 screen
- [ ] Support both ASCII and BINARY modes
- [ ] Test with x3270, wc3270, Vista TN3270

**Estimated Time**: 1-2 days

### 5.3 Zmodem Implementation (VT/Telnet)
**New Files**: `Services/Protocols/ZmodemProtocol.vb`

**Priority**: MEDIUM (more complex but widely used)

**Implementation Options**:
1. **Option A**: Use existing .NET library (search NuGet for "zmodem")
2. **Option B**: Implement pure VB.NET Zmodem (full control)
3. **Option C**: External program wrapper (DSZ.EXE, lrzsz)

**Recommended**: Try Option A first, fall back to Option B

Tasks:
- [ ] Research available .NET Zmodem libraries
- [ ] If library exists:
  - [ ] Integrate NuGet package
  - [ ] Wrap in FileTransferService
  - [ ] Test with PuTTY, SecureCRT, iTerm2
- [ ] If no library (implement from scratch):
  - [ ] Implement Zmodem frame encoding/decoding
  - [ ] Implement ZRQINIT/ZRINIT handshake
  - [ ] Implement ZFILE header transmission
  - [ ] Implement ZDATA packet streaming
  - [ ] Implement CRC16 calculation
  - [ ] Implement ZEOF and ZFIN sequences
  - [ ] Add resume capability (ZRPOS)
  - [ ] Add error recovery
- [ ] Auto-detection of Zmodem start sequence
- [ ] Progress display for Telnet users
- [ ] Batch transfer support

**Estimated Time**: 3-5 days (with library), 7-10 days (from scratch)

### 5.4 File Transfer Screens
**New Files**: `Screens/FileDownloadScreen.vb`, `Screens/FileUploadScreen.vb`

Tasks:
- [ ] File list display (both terminal types)
- [ ] File selection interface
- [ ] Protocol selection (for Telnet)
- [ ] Transfer progress display
- [ ] Batch queue management
- [ ] Upload validation and virus scanning hook

**Estimated Time**: 2-3 days

### 5.5 Optional: Additional Protocols
**New Files**: `Services/Protocols/XmodemProtocol.vb`, `Services/Protocols/YmodemProtocol.vb`

**Priority**: LOW (only if time permits)

Tasks:
- [ ] Xmodem (simple fallback)
- [ ] Ymodem (batch support)
- [ ] Ymodem-G (fast, no error checking)

**Estimated Time**: 2-3 days each

## Phase 6: Email System

### 6.1 Email Core
**Files to Convert**: Parts of BBS.C, MSGBASE.C (email functions)
**New Files**: `Services/EmailService.vb`

Tasks:
- [ ] EMAIL.DAT reading/writing
- [ ] Send email
- [ ] Read email
- [ ] Delete email
- [ ] Email forwarding

### 6.2 Email Screens
**New Files**: `Screens/EmailListScreen.vb`, `Screens/EmailReadScreen.vb`

Tasks:
- [ ] Email inbox display
- [ ] Email reader
- [ ] Email composer
- [ ] Feedback to sysop

## Phase 7: Advanced Features

### 7.1 Chains/Doors
**Files to Convert**: CHAINS.C
**New Files**: `Services/ChainService.vb`

Tasks:
- [ ] CHAIN.TXT generation
- [ ] DOOR.SYS generation
- [ ] External program execution
- [ ] Return to BBS handling

### 7.2 Network Support
**Files to Convert**: Network-related files
**New Files**: `Services/NetworkService.vb`

Tasks:
- [ ] Packet reading/writing
- [ ] Network email
- [ ] Network posts
- [ ] BBS list

### 7.3 Sysop Functions
**New Files**: `Screens/SysopMenuScreen.vb`, `Services/SysopService.vb`

Tasks:
- [ ] User editor
- [ ] System configuration
- [ ] Log viewing
- [ ] Validation queue

## Phase 8: Polish & Testing

### 8.1 Error Handling
- [ ] Comprehensive exception handling
- [ ] Graceful degradation
- [ ] Error logging
- [ ] User-friendly error messages

### 8.2 Performance
- [ ] Message caching
- [ ] User record caching
- [ ] Async optimization
- [ ] Connection pooling

### 8.3 Testing
- [ ] Unit tests for services
- [ ] Integration tests
- [ ] Load testing
- [ ] 3270 terminal compatibility testing

### 8.4 Documentation
- [ ] API documentation
- [ ] Sysop manual
- [ ] User manual
- [ ] Installation guide

## Implementation Priority

### Critical Path (Must Have)
1. User file I/O (Phase 2.1)
2. Config loading (Phase 3.1)
3. Message base core (Phase 4.1)
4. Message reading (Phase 4.2)
5. Message posting (Phase 4.3)

### Important (Should Have)
6. New user creation (Phase 2.2)
7. File directories (Phase 5.1)
8. Email core (Phase 6.1)
9. User settings (Phase 2.3)

### Nice to Have
10. File upload/download (Phase 5.2)
11. Chains/Doors (Phase 7.1)
12. Network support (Phase 7.2)
13. Sysop functions (Phase 7.3)

## Estimated Timeline

- **Phase 2 (User System)**: 2-3 days
- **Phase 3 (Configuration)**: 1-2 days
- **Phase 4 (Messages)**: 4-5 days
- **Phase 5 (Files)**: 3-4 days
- **Phase 6 (Email)**: 2-3 days
- **Phase 7 (Advanced)**: 5-7 days
- **Phase 8 (Polish)**: 3-4 days

**Total Estimated Time**: 20-28 days of focused development

## Key Design Decisions for Next Phases

### Message Storage
- Support multiple storage types (Type 2, Type 6, etc.)
- Abstract storage behind interface
- Consider migration to modern format (JSON, SQLite)

### File Paths
- Make all paths configurable
- Support both Windows and Linux paths
- Use Path.Combine for cross-platform compatibility

### Async/Await
- All I/O operations should be async
- Use CancellationToken for graceful shutdown
- Avoid blocking calls

### Telnet Input Handling
- Implement proper async line reader
- Handle control characters (Ctrl+C, etc.)
- Support ANSI color codes for Telnet

### 3270 Optimization
- Cache screen definitions
- Minimize full screen redraws
- Use Write command instead of Erase/Write when possible
