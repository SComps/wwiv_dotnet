Imports System
Imports System.IO

Namespace WWIV.Services
    ''' <summary>
    ''' System logging service
    ''' </summary>
    Public Class Logger
        Private Shared ReadOnly _logFile As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "sysop.log")
        Private Shared ReadOnly _lock As New Object()

        Public Shared Sub [Log](message As String)
            Dim logEntry = $"{DateTime.Now:MM/dd/yy HH:mm:ss} - {message}"
            Console.WriteLine(logEntry)
            
            Try
                SyncLock _lock
                    File.AppendAllText(_logFile, logEntry & Environment.NewLine)
                End SyncLock
            Catch ex As Exception
                ' Fallback
            End Try
        End Sub

        Public Shared Function GetLogLines(Optional maxLines As Integer = 100) As List(Of String)
            If Not File.Exists(_logFile) Then Return New List(Of String)()
            
            Try
                SyncLock _lock
                    Dim allLines = File.ReadAllLines(_logFile)
                    Return allLines.Reverse().Take(maxLines).ToList()
                End SyncLock
            Catch ex As Exception
                Return New List(Of String) From {$"Error reading log: {ex.Message}"}
            End Try
        End Function
    End Class
End Namespace
