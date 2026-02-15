' Example: Basic Zmodem Implementation Skeleton
' This is a simplified example showing the core concepts
' A full implementation would need more error handling and features

Imports System.IO
Imports System.Net.Sockets

Namespace WWIV.Services.Protocols

    ''' <summary>
    ''' Simplified Zmodem protocol implementation for educational purposes.
    ''' For production, consider using a tested library or implementing full spec.
    ''' </summary>
    Public Class ZmodemProtocol
        Private _stream As NetworkStream
        
        ' Zmodem Control Characters
        Private Const ZPAD As Byte = &H2A        ' '*' - Padding
        Private Const ZDLE As Byte = &H18        ' Ctrl-X - Data Link Escape
        Private Const ZBIN As Byte = &H41        ' 'A' - Binary header
        Private Const ZHEX As Byte = &H42        ' 'B' - Hex header
        
        ' Frame Types
        Private Const ZRQINIT As Byte = 0        ' Request receiver init
        Private Const ZRINIT As Byte = 1         ' Receiver init
        Private Const ZSINIT As Byte = 2         ' Sender init
        Private Const ZFILE As Byte = 4          ' File name header
        Private Const ZSKIP As Byte = 5          ' Skip this file
        Private Const ZDATA As Byte = 10         ' Data packet(s) follow
        Private Const ZEOF As Byte = 11          ' End of file
        Private Const ZFIN As Byte = 8           ' Finish session
        Private Const ZRPOS As Byte = 9          ' Resume file from position
        Private Const ZACK As Byte = 3           ' ACK to above
        
        ' Data Subpacket Types
        Private Const ZCRCW As Byte = &H80       ' CRC follows, wait for ACK
        Private Const ZCRCQ As Byte = &H81       ' CRC follows, don't wait
        Private Const ZCRCG As Byte = &H82       ' CRC follows, expect more data
        Private Const ZCRCE As Byte = &H83       ' CRC follows, end of frame
        
        Public Sub New(stream As NetworkStream)
            _stream = stream
        End Sub
        
        ''' <summary>
        ''' Send a file using Zmodem protocol
        ''' </summary>
        Public Async Function SendFileAsync(filePath As String) As Task(Of Boolean)
            Try
                ' Step 1: Initialize - send ZRQINIT
                Console.WriteLine("[Zmodem] Sending ZRQINIT...")
                Await SendHeaderAsync(ZRQINIT, {0, 0, 0, 0})
                
                ' Step 2: Wait for ZRINIT from receiver
                Console.WriteLine("[Zmodem] Waiting for ZRINIT...")
                Dim response = Await ReceiveHeaderAsync()
                If response.Type <> ZRINIT Then
                    Console.WriteLine($"[Zmodem] Error: Expected ZRINIT, got {response.Type}")
                    Return False
                End If
                
                ' Step 3: Send file information (ZFILE)
                Dim fileInfo As New FileInfo(filePath)
                Console.WriteLine($"[Zmodem] Sending ZFILE for {fileInfo.Name} ({fileInfo.Length} bytes)...")
                Await SendFileHeaderAsync(fileInfo)
                
                ' Step 4: Wait for ZRPOS (receiver ready at position)
                response = Await ReceiveHeaderAsync()
                If response.Type <> ZRPOS Then
                    Console.WriteLine($"[Zmodem] Error: Expected ZRPOS, got {response.Type}")
                    Return False
                End If
                
                Dim startPosition = BitConverter.ToInt32(response.Data, 0)
                Console.WriteLine($"[Zmodem] Receiver ready at position {startPosition}")
                
                ' Step 5: Send file data
                Console.WriteLine("[Zmodem] Sending file data...")
                Await SendFileDataAsync(filePath, startPosition)
                
                ' Step 6: Send EOF
                Console.WriteLine("[Zmodem] Sending ZEOF...")
                Dim eofData = BitConverter.GetBytes(CInt(fileInfo.Length))
                Await SendHeaderAsync(ZEOF, eofData)
                
                ' Step 7: Finish session
                Console.WriteLine("[Zmodem] Sending ZFIN...")
                Await SendHeaderAsync(ZFIN, {0, 0, 0, 0})
                
                ' Wait for final ZFIN from receiver
                response = Await ReceiveHeaderAsync()
                If response.Type = ZFIN Then
                    ' Send "OO" to complete
                    Await _stream.WriteAsync({Asc("O"), Asc("O")}, 0, 2)
                    Console.WriteLine("[Zmodem] Transfer complete!")
                    Return True
                End If
                
                Return False
                
            Catch ex As Exception
                Console.WriteLine($"[Zmodem] Error: {ex.Message}")
                Return False
            End Try
        End Function
        
        ''' <summary>
        ''' Send a Zmodem header (binary format)
        ''' </summary>
        Private Async Function SendHeaderAsync(frameType As Byte, data As Byte()) As Task
            Dim header As New List(Of Byte)
            
            ' ZPAD ZPAD ZDLE ZBIN
            header.Add(ZPAD)
            header.Add(ZPAD)
            header.Add(ZDLE)
            header.Add(ZBIN)
            
            ' Frame type
            header.Add(frameType)
            
            ' Data (4 bytes, pad if necessary)
            Dim frameData(3) As Byte
            Array.Copy(data, frameData, Math.Min(data.Length, 4))
            header.AddRange(frameData)
            
            ' CRC-16
            Dim crc = CalculateCRC16(header.Skip(4).ToArray())
            header.Add(CByte((crc >> 8) And &HFF))
            header.Add(CByte(crc And &HFF))
            
            Await _stream.WriteAsync(header.ToArray(), 0, header.Count)
            Await _stream.FlushAsync()
        End Function
        
        ''' <summary>
        ''' Receive a Zmodem header
        ''' </summary>
        Private Async Function ReceiveHeaderAsync() As Task(Of ZmodemHeader)
            Dim buffer(1024) As Byte
            Dim bytesRead = Await _stream.ReadAsync(buffer, 0, buffer.Length)
            
            ' Look for ZPAD ZPAD ZDLE sequence
            For i = 0 To bytesRead - 10
                If buffer(i) = ZPAD AndAlso buffer(i + 1) = ZPAD AndAlso buffer(i + 2) = ZDLE Then
                    Dim headerType = buffer(i + 3) ' ZBIN or ZHEX
                    Dim frameType = buffer(i + 4)
                    Dim data(3) As Byte
                    Array.Copy(buffer, i + 5, data, 0, 4)
                    
                    Return New ZmodemHeader With {
                        .Type = frameType,
                        .Data = data
                    }
                End If
            Next
            
            Throw New Exception("No valid Zmodem header found")
        End Function
        
        ''' <summary>
        ''' Send ZFILE header with filename and file info
        ''' </summary>
        Private Async Function SendFileHeaderAsync(fileInfo As FileInfo) As Task
            ' Send ZFILE header
            Await SendHeaderAsync(ZFILE, {0, 0, 0, 0})
            
            ' Send filename and file info as data subpacket
            Dim fileData As New List(Of Byte)
            
            ' Filename (null-terminated)
            fileData.AddRange(Text.Encoding.ASCII.GetBytes(fileInfo.Name))
            fileData.Add(0)
            
            ' File info: "size mtime mode serial"
            Dim info = $"{fileInfo.Length} {DateTimeOffset.Now.ToUnixTimeSeconds()} 100644 0"
            fileData.AddRange(Text.Encoding.ASCII.GetBytes(info))
            fileData.Add(0)
            
            ' Send as ZCRCW subpacket (wait for ACK)
            Await SendDataSubpacketAsync(fileData.ToArray(), ZCRCW)
        End Function
        
        ''' <summary>
        ''' Send file data in chunks
        ''' </summary>
        Private Async Function SendFileDataAsync(filePath As String, startPosition As Integer) As Task
            Using fs As New FileStream(filePath, FileMode.Open)
                fs.Seek(startPosition, SeekOrigin.Begin)
                
                ' Send ZDATA header with current position
                Dim posData = BitConverter.GetBytes(startPosition)
                Await SendHeaderAsync(ZDATA, posData)
                
                Dim buffer(1024) As Byte
                Dim bytesRead As Integer
                Dim totalSent = startPosition
                
                Do
                    bytesRead = Await fs.ReadAsync(buffer, 0, buffer.Length)
                    If bytesRead > 0 Then
                        Dim chunk(bytesRead - 1) As Byte
                        Array.Copy(buffer, chunk, bytesRead)
                        
                        ' Last chunk? Use ZCRCE, otherwise ZCRCG
                        Dim subType = If(fs.Position = fs.Length, ZCRCE, ZCRCG)
                        Await SendDataSubpacketAsync(chunk, subType)
                        
                        totalSent += bytesRead
                        
                        ' Progress
                        Dim percent = CInt((totalSent / fs.Length) * 100)
                        Console.Write($vbCr[Zmodem] Sent {totalSent}/{fs.Length} bytes ({percent}%)...")
                    End If
                Loop While bytesRead > 0
                
                Console.WriteLine()
            End Using
        End Function
        
        ''' <summary>
        ''' Send a data subpacket
        ''' </summary>
        Private Async Function SendDataSubpacketAsync(data As Byte(), subType As Byte) As Task
            Dim packet As New List(Of Byte)
            
            ' Escape special characters in data
            For Each b In data
                If b = ZDLE OrElse b = &H10 OrElse b = &H90 OrElse b = &H11 OrElse b = &H91 OrElse b = &H13 OrElse b = &H93 Then
                    packet.Add(ZDLE)
                    packet.Add(b Xor &H40)
                Else
                    packet.Add(b)
                End If
            Next
            
            ' Subpacket type
            packet.Add(ZDLE)
            packet.Add(subType)
            
            ' CRC-16 of data + subtype
            Dim crcData As New List(Of Byte)(data)
            crcData.Add(subType)
            Dim crc = CalculateCRC16(crcData.ToArray())
            packet.Add(CByte((crc >> 8) And &HFF))
            packet.Add(CByte(crc And &HFF))
            
            Await _stream.WriteAsync(packet.ToArray(), 0, packet.Count)
            Await _stream.FlushAsync()
        End Function
        
        ''' <summary>
        ''' Calculate CRC-16 (CCITT)
        ''' </summary>
        Private Function CalculateCRC16(data As Byte()) As UShort
            Dim crc As UShort = 0
            
            For Each b In data
                crc = CUShort(crc Xor (CUShort(b) << 8))
                For i = 0 To 7
                    If (crc And &H8000) <> 0 Then
                        crc = CUShort((crc << 1) Xor &H1021)
                    Else
                        crc <<= 1
                    End If
                Next
            Next
            
            Return crc
        End Function
        
        Private Class ZmodemHeader
            Public Property Type As Byte
            Public Property Data As Byte()
        End Class
    End Class
    
End Namespace

' ============================================================================
' USAGE EXAMPLE
' ============================================================================

' In FileTransferService.vb:

Public Class FileTransferService
    
    Public Async Function SendFileToTelnet(session As TelnetSessionAdapter, 
                                           filePath As String) As Task(Of Boolean)
        Try
            ' Notify user
            session.WriteLine("")
            session.WriteLine("═══════════════════════════════════════")
            session.WriteLine("  Starting Zmodem File Transfer")
            session.WriteLine("═══════════════════════════════════════")
            session.WriteLine($"File: {Path.GetFileName(filePath)}")
            session.WriteLine($"Size: {New FileInfo(filePath).Length:N0} bytes")
            session.WriteLine("")
            session.WriteLine("Your terminal should automatically")
            session.WriteLine("begin receiving the file...")
            session.WriteLine("")
            
            ' Get the network stream from the session
            Dim stream = session.GetNetworkStream()
            
            ' Create Zmodem protocol handler
            Dim zmodem As New ZmodemProtocol(stream)
            
            ' Send the file
            Dim success = Await zmodem.SendFileAsync(filePath)
            
            If success Then
                session.WriteLine("")
                session.WriteLine("✓ Transfer completed successfully!")
                session.WriteLine("")
            Else
                session.WriteLine("")
                session.WriteLine("✗ Transfer failed!")
                session.WriteLine("")
            End If
            
            Return success
            
        Catch ex As Exception
            session.WriteLine($"Error: {ex.Message}")
            Return False
        End Try
    End Function
    
End Class

' ============================================================================
' NOTES FOR PRODUCTION IMPLEMENTATION
' ============================================================================

' 1. Error Handling:
'    - Add timeout handling for receives
'    - Handle connection drops gracefully
'    - Implement retry logic for failed packets
'
' 2. Performance:
'    - Use larger buffer sizes (8KB-32KB)
'    - Implement streaming CRC calculation
'    - Consider async I/O throughout
'
' 3. Features to Add:
'    - Resume capability (handle ZRPOS properly)
'    - Batch file transfers
'    - Progress callbacks for UI updates
'    - Crash recovery
'
' 4. Testing:
'    - Test with multiple terminal emulators
'    - Test with various file sizes (1KB to 100MB+)
'    - Test resume functionality
'    - Test error conditions (disconnects, corrupted data)
'
' 5. Alternative: Use Existing Library
'    - Search NuGet for "zmodem" or "xmodem"
'    - Check GitHub for C# implementations to port
'    - Consider wrapping native libraries (lrzsz)
