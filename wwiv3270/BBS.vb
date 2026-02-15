Imports System
Imports System.Threading
Imports TN3270Framework
Imports wwiv3270.WWIV.Telnet
Imports wwiv3270.WWIV.Manager
Imports wwiv3270.WWIV.Adapters

Namespace WWIV
    Public Class BBS
        Private _tnListener As TN3270Listener
        Private _telnetListener As TelnetListener
        Private _config As New WWIV.Data.ConfigRec()
        
        Public Sub Start()
            Console.WriteLine("Initializing WWIV 3270 System...")

            ' Initialize Config (TODO: Read from file)
            _config.SystemName = "WWIV 3270 Default BBS"
            
            ' Initialize TN3270 Listener
            _tnListener = New TN3270Listener(2323)
            AddHandler _tnListener.ConnectionReceived, AddressOf OnTNConnectionReceived
            _tnListener.Start()
            Console.WriteLine($"TN3270 Listener started on port 2323")

            ' Initialize Telnet Listener
            _telnetListener = New TelnetListener(23)
            AddHandler _telnetListener.ConnectionReceived, AddressOf OnTelnetConnectionReceived
            _telnetListener.Start()
            Console.WriteLine($"Telnet Listener started on port 23")
            
            Console.WriteLine("System is Running. Press Ctrl+C to stop.")
        End Sub

        Private Sub OnTNConnectionReceived(sender As Object, e As TN3270ConnectionEventArgs)
            Dim session = e.Session
            Console.WriteLine($"New TN3270 connection from {e.RemoteEndPoint}")
            
            ' Wrap session
            Dim adapter As New TN3270SessionAdapter(session)
            SessionManager.RegisterSession(adapter)
            
            ' Start negotiation
            session.StartNegotiation()
            
            ' Wait for negotiation complete? Or handle events?
            AddHandler session.NegotiationComplete, Sub(s, args)
                Console.WriteLine("Negotiation Complete. Show Login Screen.")
                adapter.NavigateTo(New Screens.LoginScreen())
            End Sub
            
            AddHandler session.Disconnected, Sub(s, args)
                Console.WriteLine($"Session {adapter.SessionId} disconnected.")
                SessionManager.UnregisterSession(adapter.SessionId)
            End Sub
        End Sub

        Private Sub OnTelnetConnectionReceived(sender As Object, e As TelnetConnectionEventArgs)
            Dim client = e.Client
            Console.WriteLine($"New Telnet connection from {client.Client.RemoteEndPoint}")
            
            Dim adapter = New TelnetSessionAdapter(client)
            SessionManager.RegisterSession(adapter)
            
            ' Navigate to login screen
            adapter.NavigateTo(New Screens.LoginScreen())
            
            ' Begin async handling loop for this session
            Task.Run(Sub() HandleTelnetSession(adapter))
        End Sub

        Private Sub HandleTelnetSession(session As TelnetSessionAdapter)
            ' Basic loop handling for Telnet
            ' In real implementation, this would be a state machine
            Try
                ' ... loop ...
            Catch ex As Exception
                Console.WriteLine($"Telnet session error: {ex.Message}")
            Finally
                session.Disconnect()
                SessionManager.UnregisterSession(session.SessionId)
            End Try
        End Sub

    End Class
End Namespace
