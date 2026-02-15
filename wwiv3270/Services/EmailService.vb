Imports System
Imports System.IO
Imports System.Text.Json
Imports System.Collections.Concurrent
Imports wwiv3270.WWIV.Data

Namespace WWIV.Services
    ''' <summary>
    ''' Service for managing local email messages
    ''' </summary>
    Public Class EmailService
        Private ReadOnly _dataDir As String
        Private ReadOnly _emailDir As String
        
        Private Shared ReadOnly _jsonOptions As New JsonSerializerOptions With {
            .WriteIndented = True,
            .PropertyNameCaseInsensitive = True,
            .TypeInfoResolver = WWIVJsonContext.Default
        }

        Private ReadOnly _emailCache As New ConcurrentDictionary(Of Integer, List(Of EmailMessage))()
        Private ReadOnly _lock As New Object()

        Public Sub New(Optional dataDir As String = Nothing)
            _dataDir = If(dataDir, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"))
            _emailDir = Path.Combine(_dataDir, "email")
            
            If Not Directory.Exists(_emailDir) Then Directory.CreateDirectory(_emailDir)
        End Sub

        Public Function GetEmails(userNumber As Integer) As List(Of EmailMessage)
            If _emailCache.ContainsKey(userNumber) Then Return _emailCache(userNumber)
            
            Dim userFile = Path.Combine(_emailDir, $"user_{userNumber}.json")
            If File.Exists(userFile) Then
                Try
                    Dim json = File.ReadAllText(userFile)
                    Dim emails = JsonSerializer.Deserialize(Of List(Of EmailMessage))(json, _jsonOptions)
                    _emailCache(userNumber) = emails
                    Return emails
                Catch ex As Exception
                    Console.WriteLine($"Error loading email for user {userNumber}: {ex.Message}")
                    Return New List(Of EmailMessage)()
                End Try
            Else
                Dim newList = New List(Of EmailMessage)()
                _emailCache(userNumber) = newList
                Return newList
            End If
        End Function

        Public Sub SendEmail(msg As EmailMessage)
            Dim emails = GetEmails(msg.ToUserNumber)
            emails.Add(msg)
            SaveEmails(msg.ToUserNumber)
        End Sub

        Public Sub SaveEmails(userNumber As Integer)
            If Not _emailCache.ContainsKey(userNumber) Then Return
            
            Try
                Dim userFile = Path.Combine(_emailDir, $"user_{userNumber}.json")
                Dim json = JsonSerializer.Serialize(_emailCache(userNumber), _jsonOptions)
                File.WriteAllText(userFile, json)
            Catch ex As Exception
                Console.WriteLine($"Error saving email for user {userNumber}: {ex.Message}")
            End Try
        End Sub

        Public Sub DeleteEmail(userNumber As Integer, msgId As Guid)
            Dim emails = GetEmails(userNumber)
            Dim msg = emails.FirstOrDefault(Function(e) e.ID = msgId)
            If msg IsNot Nothing Then
                emails.Remove(msg)
                SaveEmails(userNumber)
            End If
        End Sub
    End Class
End Namespace
