# File Transfer Protocols - Quick Reference

## How VT/Telnet File Transfers Actually Work

### The Magic of In-Band Protocols

When you use Zmodem/Xmodem/Ymodem over a Telnet connection, the file transfer happens **over the same TCP connection** as your terminal session. Here's how:

#### Normal Terminal Mode
```
User types: ls<ENTER>
BBS receives: "ls\r\n"
BBS sends: "file1.txt\r\nfile2.txt\r\n"
Terminal displays: file1.txt
                   file2.txt
```

#### Protocol Mode (Zmodem)
```
BBS sends: "rz\r\n**\030B00..." (Zmodem start sequence)
Terminal emulator: "Oh! That's Zmodem! Switch to protocol mode!"
Terminal: Stops displaying characters, starts protocol handler
BBS sends: [binary file data with Zmodem framing]
Terminal: Receives binary data, writes to local file
BBS sends: [Zmodem end sequence]
Terminal: "Transfer done! Back to terminal mode"
BBS sends: "Transfer complete!\r\n"
Terminal displays: Transfer complete!
```

### Key Insight

The terminal emulator **recognizes special escape sequences** and automatically switches modes:

| Protocol | Start Sequence | Terminal Behavior |
|----------|---------------|-------------------|
| Zmodem | `rz` or `sz` followed by `**\030B` | Auto-detects and launches Zmodem handler |
| Ymodem | SOH (0x01) character | Waits for user to manually start receive |
| Xmodem | SOH (0x01) character | Waits for user to manually start receive |

### Why Zmodem is Superior

**Auto-start**: Terminal automatically detects and starts receiving
```
BBS: "Starting download..."
BBS: [sends Zmodem sequence]
Terminal: [automatically pops up save dialog and starts receiving]
User: [doesn't have to do anything!]
```

**Manual protocols** (Xmodem/Ymodem):
```
BBS: "Starting download. Press Ctrl+PgDn to receive..."
User: [has to manually tell terminal to start receiving]
User: [selects protocol from menu]
User: [chooses save location]
Terminal: [finally starts receiving]
```

## Zmodem Protocol Basics

### Packet Structure

```
┌─────────┬──────┬──────┬──────┬──────────┬─────┐
│ ZPAD(2) │ ZDLE │ Type │ Data │ CRC(2)   │ ... │
└─────────┴──────┴──────┴──────┴──────────┴─────┘
   **       \030    A-Z   0-4 bytes  16-bit
```

### Frame Types

| Type | Hex | Name | Purpose |
|------|-----|------|---------|
| ZRQINIT | 0x00 | Request Init | Sender asks receiver to initialize |
| ZRINIT | 0x01 | Receiver Init | Receiver ready, sends capabilities |
| ZSINIT | 0x02 | Sender Init | Sender provides info |
| ZFILE | 0x04 | File Header | Filename, size, date |
| ZSKIP | 0x05 | Skip File | Receiver already has file |
| ZDATA | 0x0A | Data Packet | File data chunk |
| ZEOF | 0x0B | End of File | File transfer complete |
| ZFIN | 0x08 | Finish | Session complete |
| ZRPOS | 0x09 | Resume Position | Resume from byte N |

### Simple Send Sequence

```
Sender                          Receiver
  |                                |
  |------ ZRQINIT ---------------->|  "Ready to send?"
  |                                |
  |<----- ZRINIT ------------------|  "Yes, I'm ready"
  |                                |
  |------ ZFILE ------------------>|  "File: test.zip, 1024 bytes"
  |                                |
  |<----- ZRPOS ------------------|  "Ready at position 0"
  |                                |
  |------ ZDATA ------------------>|  [chunk 1: bytes 0-1023]
  |                                |
  |<----- ZACK --------------------|  "Got it"
  |                                |
  |------ ZEOF ------------------->|  "File complete"
  |                                |
  |------ ZFIN ------------------->|  "All done"
  |                                |
  |<----- ZFIN --------------------|  "Acknowledged"
```

### Resume Capability

One of Zmodem's best features:

```
Transfer interrupted at 50%:
  File: bigfile.zip (10 MB)
  Received: 5 MB
  [Connection drops]

Resume transfer:
  Receiver: "I have 5 MB already"
  Receiver: [sends ZRPOS with position 5242880]
  Sender: "OK, resuming from byte 5242880"
  Sender: [sends remaining 5 MB]
```

## IND$FILE Protocol (3270)

### How It Works

IND$FILE is a **structured field** in the 3270 data stream:

```
┌────────┬─────────┬──────────┬──────────────┐
│ WSF    │ Length  │ SF Type  │ Command/Data │
└────────┴─────────┴──────────┴──────────────┘
  0xF3     2 bytes    0x88      Variable
```

### Commands

**Send file to terminal** (PUT):
```
IND$FILE PUT filename.txt ASCII
IND$FILE PUT program.exe BINARY
```

**Receive file from terminal** (GET):
```
IND$FILE GET filename.txt ASCII
IND$FILE GET program.exe BINARY
```

### Transfer Modes

| Mode | Use Case | Conversion |
|------|----------|------------|
| ASCII | Text files | EBCDIC ↔ ASCII |
| BINARY | Programs, ZIPs | No conversion |

### Example: Sending a File

```vb
' 1. Send IND$FILE command
Dim cmd = "IND$FILE PUT myfile.zip BINARY"
Dim sf As New List(Of Byte)
sf.Add(&HF3)  ' WSF
sf.Add(0)     ' Length high (filled later)
sf.Add(0)     ' Length low
sf.Add(&H88)  ' Structured field type
sf.AddRange(Encoding.ASCII.GetBytes(cmd))

' Update length
Dim len = sf.Count - 1
sf(1) = CByte((len >> 8) And &HFF)
sf(2) = CByte(len And &HFF)

session.SendRaw(sf.ToArray())

' 2. Send file data in chunks
Using fs As New FileStream("myfile.zip", FileMode.Open)
    Dim buffer(4096) As Byte
    Dim bytesRead As Integer
    Do
        bytesRead = fs.Read(buffer, 0, buffer.Length)
        If bytesRead > 0 Then
            Dim dataSf As New List(Of Byte)
            dataSf.Add(&HF3)
            dataSf.Add(0)
            dataSf.Add(0)
            dataSf.Add(&H88)
            dataSf.AddRange(buffer.Take(bytesRead))
            
            Dim dataLen = dataSf.Count - 1
            dataSf(1) = CByte((dataLen >> 8) And &HFF)
            dataSf(2) = CByte(dataLen And &HFF)
            
            session.SendRaw(dataSf.ToArray())
        End If
    Loop While bytesRead > 0
End Using

' 3. Send EOF
Dim eofSf = {&HF3, 0, 2, &H88, &HFF}
session.SendRaw(eofSf)
```

## Comparison Matrix

| Feature | IND$FILE (3270) | Zmodem (VT) | Xmodem (VT) |
|---------|----------------|-------------|-------------|
| **Auto-start** | ✅ Yes | ✅ Yes | ❌ No |
| **Resume** | ❌ No | ✅ Yes | ❌ No |
| **Batch** | ❌ No | ✅ Yes | ❌ No |
| **Speed** | ⭐⭐⭐⭐ Fast | ⭐⭐⭐⭐⭐ Fastest | ⭐⭐ Slow |
| **Error Recovery** | ⭐⭐⭐ Good | ⭐⭐⭐⭐⭐ Excellent | ⭐⭐⭐ Basic |
| **Complexity** | ⭐⭐ Simple | ⭐⭐⭐⭐ Complex | ⭐ Very Simple |
| **Terminal Support** | 3270 only | All VT/ANSI | All VT/ANSI |
| **User Experience** | ⭐⭐⭐⭐⭐ Seamless | ⭐⭐⭐⭐ Automatic | ⭐⭐ Manual |

## Implementation Recommendations

### Priority 1: IND$FILE (3270)
- Easiest to implement
- Best user experience for 3270 users
- Built into protocol
- ~200 lines of code

### Priority 2: Zmodem (VT/Telnet)
- Industry standard
- Auto-start capability
- Resume support
- ~1000 lines of code (or use library)

### Priority 3: Xmodem (VT/Telnet) - Optional
- Fallback for very old terminals
- Simple protocol
- ~300 lines of code

## Testing Tools

### 3270 Emulators with IND$FILE Support
- **x3270** (Linux/Unix) - Free, excellent IND$FILE support
- **wc3270** (Windows) - Free, command-line
- **Vista TN3270** (Windows) - Commercial, full-featured
- **TN3270 Plus** (Windows) - Free, good support

### VT Emulators with Zmodem Support
- **PuTTY** (Windows) - Requires external sz/rz
- **SecureCRT** (Windows/Mac) - Built-in Zmodem
- **iTerm2** (Mac) - Built-in Zmodem with triggers
- **Tera Term** (Windows) - Built-in Zmodem
- **minicom** (Linux) - Built-in Zmodem

### Command-Line Tools
```bash
# Linux/Mac: Install lrzsz package
sudo apt-get install lrzsz    # Debian/Ubuntu
brew install lrzsz             # macOS

# Test Zmodem send
sz filename.zip

# Test Zmodem receive
rz
```

## Common Issues and Solutions

### Issue: "Zmodem not starting automatically"
**Solution**: Ensure you're sending the correct start sequence:
```vb
' Correct Zmodem start for SEND (sz)
Dim start = "rz" & vbCr & "**" & Chr(&H18) & "B00000000000000"

' Correct Zmodem start for RECEIVE (rz)
Dim start = "sz" & vbCr & "**" & Chr(&H18) & "B00000000000000"
```

### Issue: "Binary files corrupted"
**Solution**: Ensure stream is in binary mode, not text mode:
```vb
' Wrong - will corrupt binary data
Dim writer As New StreamWriter(stream)

' Right - preserves binary data
stream.Write(bytes, 0, bytes.Length)
```

### Issue: "IND$FILE not recognized by emulator"
**Solution**: Check structured field format:
```vb
' Must be exact format
WSF (0xF3) + Length (2 bytes) + Type (0x88) + Command
```

## Resources

### Zmodem Specification
- Original: [ZMODEM.DOC by Chuck Forsberg](https://gallium.inria.fr/~doligez/zmodem/zmodem.txt)
- Modern: [Zmodem Protocol](https://en.wikipedia.org/wiki/ZMODEM)

### IND$FILE Specification
- IBM: [3270 Data Stream Programmer's Reference](https://www.ibm.com/docs/en/zos/2.4.0?topic=reference-3270-data-stream-programmers)
- IND$FILE: Search IBM docs for "IND$FILE Protocol"

### .NET Libraries
- Search NuGet: `zmodem`, `xmodem`, `file transfer protocol`
- GitHub: Search for "zmodem c#" or "zmodem .net"
