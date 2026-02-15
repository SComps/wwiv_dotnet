Imports System
Imports System.Net.Sockets
Imports System.Text
Imports System.IO
Imports System.Threading.Tasks
Imports System.Collections.Generic
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Data

Namespace WWIV.Telnet
    ''' <summary>
    ''' Adapter for standard Telnet sessions.
    ''' Handles Telnet negotiation (IAC) and basic NVT behavior.
    ''' </summary>
    Public Class TelnetSessionAdapter
        Implements ISession

        Private _client As TcpClient
        Private _stream As NetworkStream
        Private _sessionId As Guid
        Private _user As UserRecord
        Private _currentSub As Integer = 0
        Private _currentDir As Integer = 0
        Private _currentScreen As IScreen
        
        ' Telnet Protocol Constants
        Private Const IAC As Byte = 255
        Private Const DONT As Byte = 254
        Private Const DO_CMD As Byte = 253
        Private Const WONT As Byte = 252
        Private Const WILL As Byte = 251
        Private Const SB As Byte = 250
        Private Const SE As Byte = 240
        
        ' Telnet Options
        Private Const OPT_BINARY As Byte = 0
        Private Const OPT_ECHO As Byte = 1
        Private Const OPT_SGA As Byte = 3
        Private Const OPT_TERM_TYPE As Byte = 24
        Private Const OPT_NAWS As Byte = 31

        Public Sub New(client As TcpClient)
            _client = client
            _stream = client.GetStream()
            _sessionId = Guid.NewGuid()
            
            ' Send initial negotiation
            SendNegotiation()
        End Sub

        Private Sub SendNegotiation()
            Try
                ' Request client to suppress go-ahead and handle echo locally
                Dim bytes() As Byte = {
                    IAC, WILL, OPT_SGA,
                    IAC, WILL, OPT_ECHO,
                    IAC, DONT, OPT_TERM_TYPE,
                    IAC, DONT, OPT_NAWS
                }
                _stream.Write(bytes, 0, bytes.Length)
            Catch
            End Try
        End Sub

        Public ReadOnly Property SessionId As Guid Implements ISession.SessionId
            Get
                Return _sessionId
            End Get
        End Property

        Public Property User As UserRecord Implements ISession.User
            Get
                Return _user
            End Get
            Set(value As UserRecord)
                _user = value
            End Set
        End Property

        Public Property CurrentSub As Integer Implements ISession.CurrentSub
            Get
                Return _currentSub
            End Get
            Set(value As Integer)
                _currentSub = value
            End Set
        End Property

        Public Property CurrentDir As Integer Implements ISession.CurrentDir
            Get
                Return _currentDir
            End Get
            Set(value As Integer)
                _currentDir = value
            End Set
        End Property

        Public Sub Write(text As String) Implements ISession.Write
            If String.IsNullOrEmpty(text) Then Return
            Try
                Dim bytes = Encoding.UTF8.GetBytes(text)
                _stream.Write(bytes, 0, bytes.Length)
            Catch ex As Exception
                Console.WriteLine($"Error writing to session {_sessionId}: {ex.Message}")
            End Try
        End Sub

        Public Sub WriteLine(text As String) Implements ISession.WriteLine
            Write(text & vbCrLf)
        End Sub

        Public Sub NavigateTo(screen As IScreen) Implements ISession.NavigateTo
            _currentScreen = screen
            screen.Activate(Me)
        End Sub

        Public Sub Disconnect() Implements ISession.Disconnect
            Try
                _client.Close()
            Catch
            End Try
        End Sub

        Public Function GetStream() As NetworkStream
            Return _stream
        End Function

        Public Function IsConnected() As Boolean
            Return _client IsNot Nothing AndAlso _client.Connected
        End Function

        ''' <summary>
        ''' Main input loop for Telnet. 
        ''' Reads bytes and handles Telnet IAC sequences.
        ''' </summary>
        Public Async Function RunSessionLoop() As Task
            Dim buffer(4096) As Byte
            Dim lineBuffer As New List(Of Byte)()
            
            Try
                While IsConnected()
                    Dim count = Await _stream.ReadAsync(buffer, 0, buffer.Length)
                    If count = 0 Then Exit While
                    
                    Dim i = 0
                    While i < count
                        Dim b = buffer(i)
                        
                        If b = IAC Then
                            ' Telnet command sequence (IAC + CMD [+ OPT])
                            If i + 1 < count Then
                                Dim cmd = buffer(i + 1)
                                Select Case cmd
                                    Case WILL, WONT, DO_CMD, DONT
                                        i += 3 ' 3-byte command
                                    Case SB
                                        ' Sub-negotiation: skip until SE
                                        i += 2
                                        While i < count AndAlso buffer(i) <> SE
                                            i += 1
                                        End While
                                        i += 1
                                    Case IAC
                                        ' Literal 255
                                        lineBuffer.Add(255)
                                        i += 2
                                    Case Else
                                        i += 2 ' 2-byte command
                                End Select
                            Else
                                ' IAC at end of buffer? Just skip it for now.
                                i += 1
                            End If
                        ElseIf b = 10 Then ' LF
                            ' End of line
                            If lineBuffer.Count > 0 Then
                                Dim line = Encoding.UTF8.GetString(lineBuffer.ToArray()).Trim()
                                HandleInput(line)
                                lineBuffer.Clear()
                            End If
                            i += 1
                        ElseIf b = 13 Then ' CR
                            ' Ignore CR, wait for LF
                            i += 1
                        Else
                            ' Printable ASCII or UTF-8 byte
                            ' Filter out common non-printable low ASCII control codes
                            If b >= 32 OrElse b = 9 Then
                                lineBuffer.Add(b)
                            End If
                            i += 1
                        End If
                    End While
                End While
            Catch ex As Exception
                Console.WriteLine($"Telnet session {_sessionId} error: {ex.Message}")
            Finally
                Disconnect()
            End Try
        End Function

        Public Sub HandleInput(input As String)
            If _currentScreen IsNot Nothing Then
                _currentScreen.HandleInput(Me, input)
            End If
        End Sub
    End Class
End Namespace
