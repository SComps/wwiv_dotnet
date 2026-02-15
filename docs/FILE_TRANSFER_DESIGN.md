# File Transfer Architecture for WWIV 3270

## Overview

WWIV 3270 will support two distinct file transfer methods based on terminal type:

1. **IND$FILE** for TN3270 terminals (native 3270 protocol)
2. **External Protocols** (Zmodem/Xmodem/Ymodem) for VT/Telnet terminals

## TN3270: IND$FILE Method

### What is IND$FILE?

IND$FILE is a file transfer protocol built into the 3270 data stream. It's supported by most 3270 emulators (x3270, wc3270, etc.) and allows binary file transfers without leaving the 3270 session.

### Implementation Approach

```vb
' Send file to 3270 terminal
Public Sub SendFileToTerminal(session As TN3270Session, filePath As String)
    ' IND$FILE PUT command
    ' Format: IND$FILE PUT filename BINARY
    Dim command = $"IND$FILE PUT {Path.GetFileName(filePath)} BINARY"
    
    ' Send as structured field
    Dim sfData As New List(Of Byte)
    sfData.Add(&H88) ' Structured Field ID
    sfData.AddRange(Encoding.ASCII.GetBytes(command))
    
    session.WriteStructuredField(sfData.ToArray())
    
    ' Read file and send in chunks
    Using fs As New FileStream(filePath, FileMode.Open)
        Dim buffer(4096) As Byte
        Dim bytesRead As Integer
        Do
            bytesRead = fs.Read(buffer, 0, buffer.Length)
            If bytesRead > 0 Then
                session.WriteStructuredField(buffer.Take(bytesRead).ToArray())
            End If
        Loop While bytesRead > 0
    End Using
    
    ' Send end-of-file marker
    session.WriteStructuredField({&HFF})
End Sub

' Receive file from 3270 terminal
Public Sub ReceiveFileFromTerminal(session As TN3270Session, filename As String)
    ' IND$FILE GET command
    Dim command = $"IND$FILE GET {filename} BINARY"
    
    ' Send command and wait for structured field responses
    AddHandler session.StructuredFieldReceived, AddressOf OnFileDataReceived
    
    session.WriteStructuredField(Encoding.ASCII.GetBytes(command))
End Sub
```

### Advantages of IND$FILE
- ✅ No external programs needed
- ✅ Works within the 3270 session
- ✅ Binary safe
- ✅ Built into most emulators
- ✅ Can show progress in 3270 screen

## VT/Telnet: External Protocol Method

### How External Protocols Work

External protocols (Zmodem, Xmodem, Ymodem) operate **in-band** over the same connection as the terminal. The BBS temporarily switches from "terminal mode" to "protocol mode":

1. **BBS announces**: "Starting Zmodem transfer..."
2. **BBS sends**: Special escape sequence to trigger protocol
3. **Terminal emulator**: Detects sequence, launches protocol handler
4. **Transfer occurs**: Over the same TCP connection
5. **Protocol ends**: Terminal returns to normal mode
6. **BBS resumes**: Normal terminal operation

### Protocol Comparison

| Protocol | Speed | Batch | Resume | Error Recovery | Popularity |
|----------|-------|-------|--------|----------------|------------|
| **Zmodem** | Fast | Yes | Yes | Excellent | ⭐⭐⭐⭐⭐ Most popular |
| Ymodem-G | Fastest | Yes | No | None (requires reliable connection) | ⭐⭐⭐ |
| Ymodem | Medium | Yes | No | Good | ⭐⭐⭐ |
| Xmodem-1K | Medium | No | No | Good | ⭐⭐ |
| Xmodem | Slow | No | No | Basic | ⭐ Legacy |

**Recommendation**: Implement **Zmodem** first (most widely supported and feature-rich).

### Zmodem Implementation Strategy

#### Option 1: Use Existing .NET Library (Recommended)

There are .NET implementations of Zmodem available. We can integrate one:

```vb
' Using a hypothetical Zmodem library
Public Async Function SendFileZmodem(session As TelnetSessionAdapter, 
                                     filePath As String) As Task
    ' Notify user
    session.WriteLine("Starting Zmodem transfer...")
    session.WriteLine("Your terminal should automatically begin receiving.")
    
    ' Send Zmodem initialization sequence
    ' ZRQINIT: "rz\r**\030B00000000000000\r\n"
    Dim zmodem As New ZmodemSender(session.GetStream())
    
    Try
        Await zmodem.SendFileAsync(filePath)
        session.WriteLine("Transfer complete!")
    Catch ex As Exception
        session.WriteLine($"Transfer failed: {ex.Message}")
    End Try
End Function

Public Async Function ReceiveFileZmodem(session As TelnetSessionAdapter, 
                                        savePath As String) As Task
    session.WriteLine("Starting Zmodem receive...")
    session.WriteLine("Please start your Zmodem send now.")
    
    Dim zmodem As New ZmodemReceiver(session.GetStream())
    
    Try
        Await zmodem.ReceiveFileAsync(savePath)
        session.WriteLine("Upload complete!")
    Catch ex As Exception
        session.WriteLine($"Upload failed: {ex.Message}")
    End Try
End Function
```

#### Option 2: External Program (Like Original WWIV)

Original WWIV used external programs (DSZ.EXE, GSZ.EXE). We can do the same:

```vb
Public Async Function SendFileWithExternalProtocol(
    session As TelnetSessionAdapter, 
    filePath As String, 
    protocol As String) As Task
    
    ' Get protocol configuration
    Dim protocolExe = GetProtocolCommand(protocol, "send", filePath)
    ' Example: "C:\BBS\DSZ.EXE port 23 sz {filePath}"
    
    ' Temporarily release the socket to the external program
    Dim socket = session.GetSocket()
    
    ' Run external protocol
    Dim psi As New ProcessStartInfo With {
        .FileName = protocolExe.Program,
        .Arguments = protocolExe.Arguments,
        .UseShellExecute = False,
        .RedirectStandardInput = True,
        .RedirectStandardOutput = True
    }
    
    Using process = Process.Start(psi)
        ' Pipe socket to process
        Await PipeSocketToProcessAsync(socket, process)
        Await process.WaitForExitAsync()
    End Using
    
    ' Resume normal terminal operation
    session.WriteLine("Transfer complete. Press ENTER to continue.")
End Function
```

**Challenges with External Programs**:
- ❌ Requires external executables (DSZ, lsz, etc.)
- ❌ Platform-specific (Windows vs Linux)
- ❌ Socket handoff is complex
- ❌ Less control over the transfer

#### Option 3: Pure .NET Implementation (Most Control)

Implement Zmodem protocol directly in VB.NET:

```vb
Public Class ZmodemProtocol
    Private _stream As NetworkStream
    
    ' Zmodem constants
    Private Const ZPAD = &H2A    ' '*'
    Private Const ZDLE = &H18    ' Ctrl-X
    Private Const ZBIN = &H41    ' 'A'
    Private Const ZHEX = &H42    ' 'B'
    
    ' Frame types
    Private Const ZRQINIT = 0    ' Request init
    Private Const ZRINIT = 1     ' Receive init
    Private Const ZSINIT = 2     ' Send init
    Private Const ZFILE = 4      ' File name
    Private Const ZDATA = 10     ' Data packet
    Private Const ZEOF = 11      ' End of file
    Private Const ZFIN = 8       ' Finish
    
    Public Async Function SendFileAsync(filePath As String) As Task
        ' 1. Send ZRQINIT
        Await SendHeaderAsync(ZRQINIT, {0, 0, 0, 0})
        
        ' 2. Wait for ZRINIT from receiver
        Dim response = Await ReceiveHeaderAsync()
        If response.Type <> ZRINIT Then
            Throw New Exception("Invalid response from receiver")
        End If
        
        ' 3. Send ZFILE with filename
        Dim fileInfo = New FileInfo(filePath)
        Await SendFileHeaderAsync(fileInfo)
        
        ' 4. Wait for ZRPOS (receiver ready)
        response = Await ReceiveHeaderAsync()
        
        ' 5. Send file data in ZDATA frames
        Using fs As New FileStream(filePath, FileMode.Open)
            Await SendFileDataAsync(fs)
        End Using
        
        ' 6. Send ZEOF
        Await SendHeaderAsync(ZEOF, BitConverter.GetBytes(fileInfo.Length))
        
        ' 7. Send ZFIN
        Await SendHeaderAsync(ZFIN, {0, 0, 0, 0})
    End Function
    
    Private Async Function SendHeaderAsync(type As Byte, data As Byte()) As Task
        ' ZPAD + ZPAD + ZDLE + ZBIN + type + data[4] + CRC[2]
        Dim header As New List(Of Byte)
        header.Add(ZPAD)
        header.Add(ZPAD)
        header.Add(ZDLE)
        header.Add(ZBIN)
        header.Add(type)
        header.AddRange(data.Take(4))
        
        ' Calculate CRC16
        Dim crc = CalculateCRC16(header.Skip(4).ToArray())
        header.AddRange(BitConverter.GetBytes(crc))
        
        Await _stream.WriteAsync(header.ToArray(), 0, header.Count)
    End Function
    
    ' ... more implementation
End Class
```

### Recommended Approach: Hybrid

1. **For .NET Core**: Use a NuGet package if available (e.g., search for "Zmodem .NET")
2. **If no package exists**: Implement basic Zmodem in pure VB.NET
3. **Fallback**: Support external programs for advanced users

## Implementation Plan

### Phase 1: TN3270 IND$FILE (Easier)
```vb
' File: Services/FileTransferService.vb
Public Class FileTransferService
    Public Async Function SendFile3270(session As TN3270SessionAdapter, 
                                       filePath As String) As Task
        ' Implement IND$FILE PUT
    End Function
    
    Public Async Function ReceiveFile3270(session As TN3270SessionAdapter, 
                                          filename As String, 
                                          savePath As String) As Task
        ' Implement IND$FILE GET
    End Function
End Class
```

### Phase 2: Telnet Zmodem (More Complex)
```vb
' File: Services/Protocols/ZmodemProtocol.vb
Public Class ZmodemProtocol
    Public Async Function SendAsync(stream As NetworkStream, 
                                    filePath As String) As Task
        ' Full Zmodem send implementation
    End Function
    
    Public Async Function ReceiveAsync(stream As NetworkStream, 
                                       savePath As String) As Task
        ' Full Zmodem receive implementation
    End Function
End Class

' File: Services/FileTransferService.vb (extended)
Public Async Function SendFileTelnet(session As TelnetSessionAdapter, 
                                     filePath As String, 
                                     protocol As TransferProtocol) As Task
    Select Case protocol
        Case TransferProtocol.Zmodem
            Dim zmodem As New ZmodemProtocol()
            Await zmodem.SendAsync(session.GetStream(), filePath)
        Case TransferProtocol.Ymodem
            ' Future implementation
        Case TransferProtocol.Xmodem
            ' Future implementation
    End Select
End Function
```

### Phase 3: File Transfer Screens
```vb
' File: Screens/FileTransferScreen.vb
Public Class FileTransferScreen
    Implements IScreen
    
    Public Sub Activate(session As ISession) Implements IScreen.Activate
        If TypeOf session Is TN3270SessionAdapter Then
            ' Show file list with IND$FILE download option
            RenderFileList3270(DirectCast(session, TN3270SessionAdapter))
        Else
            ' Show file list with protocol selection
            RenderFileListTelnet(session)
        End If
    End Sub
    
    Public Sub HandleInput(session As ISession, input As Object) Implements IScreen.HandleInput
        ' Handle file selection and transfer initiation
    End Sub
End Class
```

## User Experience

### TN3270 Experience
```
╔════════════════════════════════════════════════════════════════════════════╗
║                          WWIV File Transfer                                ║
╚════════════════════════════════════════════════════════════════════════════╝

Directory: Games

Sel  Filename          Size      Date       Downloads  Description
───  ────────────────  ────────  ─────────  ─────────  ─────────────────────
 _   DOOM.ZIP          2.4 MB    01/15/26   142        Classic FPS Game
 _   QUAKE.ZIP         3.1 MB    01/20/26   89         Quake Shareware
 _   DUKE3D.ZIP        1.8 MB    02/01/26   67         Duke Nukem 3D Demo

Enter selection (or 'Q' to quit): 1_

[User presses ENTER]

Transferring DOOM.ZIP via IND$FILE...
████████████████████░░░░░░░░░░  65%  1.56 MB / 2.4 MB

[File automatically downloads to user's PC via 3270 emulator]
```

### Telnet/VT Experience
```
╔════════════════════════════════════════════════════════════════════════════╗
║                          WWIV File Transfer                                ║
╚════════════════════════════════════════════════════════════════════════════╝

Directory: Games

 1. DOOM.ZIP          2.4 MB    01/15/26   142 DLs   Classic FPS Game
 2. QUAKE.ZIP         3.1 MB    01/20/26   89 DLs    Quake Shareware
 3. DUKE3D.ZIP        1.8 MB    02/01/26   67 DLs    Duke Nukem 3D Demo

Select file (1-3) or Q to quit: 1

File: DOOM.ZIP (2.4 MB)

Select Protocol:
  [Z] Zmodem (Recommended)
  [Y] Ymodem
  [X] Xmodem
  [C] Cancel

Protocol: Z

Starting Zmodem transfer...
Your terminal should automatically begin receiving the file.

rz
**B00000000000000
[Binary data stream...]

Transfer complete! 2,457,600 bytes sent.

Press ENTER to continue...
```

## Testing Strategy

### TN3270 Testing
1. Test with x3270 on Linux
2. Test with wc3270 on Windows
3. Test with various file sizes (1KB, 1MB, 10MB)
4. Test binary files (ZIP, EXE, JPG)
5. Test text files with proper conversion

### Telnet Testing
1. Test with PuTTY (Windows)
2. Test with Terminal.app (macOS)
3. Test with various Zmodem implementations (lrzsz, etc.)
4. Test interrupted transfers (resume capability)
5. Test batch transfers

## Configuration

```json
{
  "FileTransfer": {
    "TN3270": {
      "Enabled": true,
      "MaxFileSize": 10485760,  // 10 MB
      "AllowBinary": true
    },
    "Telnet": {
      "Protocols": {
        "Zmodem": {
          "Enabled": true,
          "MaxFileSize": 10485760,
          "BlockSize": 1024,
          "Timeout": 30
        },
        "Ymodem": {
          "Enabled": false
        },
        "Xmodem": {
          "Enabled": false
        }
      }
    },
    "UploadPath": "C:\\WWIV\\uploads",
    "TempPath": "C:\\WWIV\\temp",
    "VirusScan": {
      "Enabled": false,
      "Command": "C:\\AV\\scan.exe"
    }
  }
}
```

## Summary

- **TN3270**: Use IND$FILE (built into 3270 protocol, easy to implement)
- **Telnet/VT**: Use Zmodem (industry standard, widely supported)
- **Implementation**: Start with IND$FILE, then add Zmodem
- **Libraries**: Search for existing .NET Zmodem implementations first
- **Fallback**: Implement basic Zmodem in pure VB.NET if needed

This approach gives the best user experience for both terminal types while maintaining compatibility with the widest range of terminal emulators.
