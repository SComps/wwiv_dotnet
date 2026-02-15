Imports System
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Collections.Concurrent

Namespace WWIV.Telnet
    Public Class TelnetConnectionEventArgs
        Inherits EventArgs
        Public ReadOnly Property Client As TcpClient
        Public Sub New(client As TcpClient)
            Me.Client = client
        End Sub
    End Class

    Public Class TelnetListener
        Private _listener As TcpListener
        Private _isRunning As Boolean
        Private _port As Integer

        Public Event ConnectionReceived(sender As Object, e As TelnetConnectionEventArgs)

        Public Sub New(port As Integer)
            _port = port
            _listener = New TcpListener(IPAddress.Any, port)
        End Sub

        Public Sub Start()
            _listener.Start()
            _isRunning = True
            Task.Run(AddressOf ListenLoop)
        End Sub

        Public Sub StopListening()
            _isRunning = False
            _listener.Stop()
        End Sub

        Private Async Sub ListenLoop()
            While _isRunning
                Try
                    Dim client = Await _listener.AcceptTcpClientAsync()
                    RaiseEvent ConnectionReceived(Me, New TelnetConnectionEventArgs(client))
                Catch ex As Exception
                    If _isRunning Then Console.WriteLine($"Telnet Listener error: {ex.Message}")
                End Try
            End While
        End Sub
    End Class
End Namespace
