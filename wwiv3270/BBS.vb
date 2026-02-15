Imports System
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports TN3270Framework
Imports wwiv3270.WWIV.Telnet
Imports wwiv3270.WWIV.Manager
Imports wwiv3270.WWIV.Adapters
Imports wwiv3270.WWIV.Services

Namespace WWIV
    Public Class BBS
        Private _tnListener As TN3270Listener
        Private _telnetListener As TelnetListener
        Private _configService As Services.ConfigService
        
        Public Sub Start()
            Logger.Log("Initializing WWIV 3270 System...")

            ' Initialize Config
            _configService = New Services.ConfigService()
            Logger.Log($"System Name: {_configService.Config.SystemName}")
            
            ' Initialize TN3270 Listener
            _tnListener = New TN3270Listener(2323)
            AddHandler _tnListener.ConnectionReceived, AddressOf OnTNConnectionReceived
            _tnListener.Start()
            Logger.Log($"TN3270 Listener started on port 2323")

            ' Initialize Telnet Listener
            _telnetListener = New TelnetListener(23)
            AddHandler _telnetListener.ConnectionReceived, AddressOf OnTelnetConnectionReceived
            _telnetListener.Start()
            Logger.Log($"Telnet Listener started on port 23")
            
            Logger.Log("System is Running. Press Ctrl+C to stop.")
        End Sub

        Private Sub OnTNConnectionReceived(sender As Object, e As TN3270ConnectionEventArgs)
            Dim session = e.Session
            Logger.Log($"New TN3270 connection from {e.RemoteEndPoint}")
            
            ' Wrap session
            Dim adapter As New TN3270SessionAdapter(session)
            SessionManager.RegisterSession(adapter)
            
            ' Increment system calls
            _configService.IncrementCalls()
            
            ' Start negotiation
            session.StartNegotiation()
            
            ' Wait for negotiation complete? Or handle events?
            AddHandler session.NegotiationComplete, Sub(s, args)
                Logger.Log($"Negotiation Complete for {e.RemoteEndPoint}. Show Login Screen.")
                adapter.NavigateTo(New Screens.LoginScreen())
            End Sub
            
            AddHandler session.Disconnected, Sub(s, args)
                Logger.Log($"Session {adapter.SessionId} disconnected.")
                SessionManager.UnregisterSession(adapter.SessionId)
            End Sub
        End Sub

        Private Sub OnTelnetConnectionReceived(sender As Object, e As TelnetConnectionEventArgs)
            Dim client = e.Client
            Console.WriteLine($"New Telnet connection from {client.Client.RemoteEndPoint}")
            
            Dim adapter = New TelnetSessionAdapter(client)
            SessionManager.RegisterSession(adapter)
            
            ' Increment system calls
            _configService.IncrementCalls()
            
            ' Navigate to login screen
            adapter.NavigateTo(New Screens.LoginScreen())
            
            ' Begin async handling loop for this session
            Task.Run(Function() HandleTelnetSession(adapter))
        End Sub

        Private Async Function HandleTelnetSession(session As TelnetSessionAdapter) As Task
            ' Basic loop handling for Telnet
            Try
                Dim stream = session.GetStream()
                Dim reader = New StreamReader(stream, System.Text.Encoding.ASCII)
                
                While session.IsConnected()
                    Dim line = Await reader.ReadLineAsync()
                    If line Is Nothing Then Exit While
                    
                    ' Pass input to current screen
                    session.HandleInput(line)
                End While
            Catch ex As Exception
                Console.WriteLine($"Telnet session error: {ex.Message}")
            Finally
                session.Disconnect()
                SessionManager.UnregisterSession(session.SessionId)
                Logger.Log($"Telnet session {session.SessionId} closed.")
            End Try
        End Function

    End Class
End Namespace
