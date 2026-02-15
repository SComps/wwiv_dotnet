Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Data

Namespace WWIV.Adapters
    Public Class TN3270SessionAdapter
        Implements ISession

        Private _tnSession As TN3270Session
        Private _sessionId As Guid
        Private _user As UserRecord
        Private _currentSub As Integer = 0
        Private _currentDir As Integer = 0
        Private _currentScreen As IScreen
        Public Sub New(tnSession As TN3270Session)
            _tnSession = tnSession
            _sessionId = Guid.NewGuid()
            
            AddHandler _tnSession.AidKeyReceived, AddressOf OnAidKey
        End Sub

        Public ReadOnly Property TN3270Session As TN3270Session
            Get
                Return _tnSession
            End Get
        End Property

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
            ' Simple write to current cursor position (or append)
            ' In 3270, this is tricky. We probably want to write to a "console" area.
            ' For now, we'll just write to a new field at the bottom.
            _tnSession.WriteText(_tnSession.Rows, 1, text)
            _tnSession.ShowScreen(False)
        End Sub

        Public Sub WriteLine(text As String) Implements ISession.WriteLine
            Write(text) 
            ' 3270 doesn't really have "lines" in a stream sense. 
            ' We might need a virtual console implementation later.
        End Sub

        Public Sub NavigateTo(screen As IScreen) Implements ISession.NavigateTo
            _currentScreen = screen
            _tnSession.ClearFields()
            screen.Activate(Me)
        End Sub

        Public Sub Disconnect() Implements ISession.Disconnect
            _tnSession.Close()
        End Sub
        
        Private Sub OnAidKey(sender As Object, e As AidKeyEventArgs)
            ' Handle Enter, PF keys, etc.
            Console.WriteLine($"Session {_sessionId} pressed key {e.AidKey:X2}")
            If _currentScreen IsNot Nothing Then
                _currentScreen.HandleInput(Me, e)
            End If
        End Sub

    End Class
End Namespace
