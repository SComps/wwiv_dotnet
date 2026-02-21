Imports System
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports TN3270Framework
Imports wwiv3270.WWIV.Telnet
Imports wwiv3270.WWIV.Manager
Imports wwiv3270.WWIV.Adapters
Imports WWIV.Services

Namespace WWIV
    Public Class BBS
        Private _tnListener As TN3270Listener
        Private _telnetListener As TelnetListener
        Private _configService As ConfigService
        
        Public Sub Start()
            Logger.Log("Initializing WWIV 3270 System...")

            ' Initialize Config
            _configService = New ConfigService()
            Logger.Log($"System Name: {_configService.Config.SystemName}")
            
            ' Initialize TN3270 Listener
            _tnListener = New TN3270Listener(_configService.Config.TN3270Port)
            AddHandler _tnListener.ConnectionReceived, AddressOf OnTNConnectionReceived
            _tnListener.Start()
            Logger.Log($"TN3270 Listener started on port {_configService.Config.TN3270Port}")

            ' Initialize Telnet Listener
            _telnetListener = New TelnetListener(_configService.Config.TelnetPort)
            AddHandler _telnetListener.ConnectionReceived, AddressOf OnTelnetConnectionReceived
            _telnetListener.Start()
            Logger.Log($"Telnet Listener started on port {_configService.Config.TelnetPort}")
            
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
            ' Use the refined session loop that handles Telnet negotiation
            Try
                Await session.RunSessionLoop()
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
