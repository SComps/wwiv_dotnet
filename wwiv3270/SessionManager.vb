Imports System
Imports System.Collections.Concurrent
Imports wwiv3270.WWIV.Core

Namespace WWIV.Manager
    Public Class SessionManager
        Private Shared _sessions As New ConcurrentDictionary(Of Guid, ISession)

        Public Shared Sub RegisterSession(session As ISession)
            _sessions.TryAdd(session.SessionId, session)
            Console.WriteLine($"Session {session.SessionId} registered.")
        End Sub

        Public Shared Sub UnregisterSession(sessionId As Guid)
            Dim removedSession As ISession = Nothing
            _sessions.TryRemove(sessionId, removedSession)
            Console.WriteLine($"Session {sessionId} removed.")
        End Sub

        Public Shared Function GetAllSessions() As IEnumerable(Of ISession)
            Return _sessions.Values
        End Function
    End Class
End Namespace
