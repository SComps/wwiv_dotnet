Imports System
Imports System.Net.Sockets
Imports System.Text
Imports System.IO
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Data

Namespace WWIV.Telnet
    Public Class TelnetSessionAdapter
        Implements ISession

        Private _client As TcpClient
        Private _stream As NetworkStream
        Private _reader As StreamReader
        Private _writer As StreamWriter
        Private _sessionId As Guid
        Private _user As UserRecord
        Private _currentSub As Integer = 0
        Private _currentDir As Integer = 0
        Private _currentScreen As IScreen

        Public Sub New(client As TcpClient)
            _client = client
            _stream = client.GetStream()
            _reader = New StreamReader(_stream, Encoding.ASCII)
            _writer = New StreamWriter(_stream, Encoding.ASCII) With { .AutoFlush = True }
            _sessionId = Guid.NewGuid()
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
            Try
                _writer.Write(text)
            Catch ex As Exception
                Console.WriteLine($"Error writing to session {_sessionId}: {ex.Message}")
            End Try
        End Sub

        Public Sub WriteLine(text As String) Implements ISession.WriteLine
            Try
                _writer.WriteLine(text)
            Catch ex As Exception
                Console.WriteLine($"Error writing line to session {_sessionId}: {ex.Message}")
            End Try
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
    End Class
End Namespace
