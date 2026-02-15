Imports System
Imports System.IO
Imports System.Xml.Linq
Imports System.Collections.Concurrent
Imports System.Linq
Imports wwiv3270.WWIV.Data

Namespace WWIV.Services
    ''' <summary>
    ''' Service for managing message sub-boards and messages using XML
    ''' </summary>
    Public Class BoardService
        Private ReadOnly _dataDir As String
        Private ReadOnly _subsFile As String
        Private ReadOnly _msgsDir As String
        
        Private _subList As List(Of SubBoard)
        Private ReadOnly _msgCache As New ConcurrentDictionary(Of Integer, List(Of MessageHeader))()
        Private ReadOnly _lock As New Object()

        Public Sub New(Optional dataDir As String = Nothing)
            _dataDir = If(dataDir, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"))
            _msgsDir = Path.Combine(_dataDir, "msgs")
            _subsFile = Path.Combine(_dataDir, "subs.xml")
            
            If Not Directory.Exists(_msgsDir) Then Directory.CreateDirectory(_msgsDir)
            
            LoadSubs()
        End Sub

        Private Sub LoadSubs()
            SyncLock _lock
                If File.Exists(_subsFile) Then
                    Try
                        Dim doc = XDocument.Load(_subsFile)
                        _subList = New List(Of SubBoard)()
                        For Each el In doc.Root.Elements("SubBoard")
                            _subList.Add(SubBoard.FromXml(el))
                        Next
                    Catch ex As Exception
                        Console.WriteLine($"Error loading sub-boards: {ex.Message}")
                        _subList = New List(Of SubBoard)()
                    End Try
                Else
                    _subList = New List(Of SubBoard)()
                    ' Add a default sub
                    _subList.Add(New SubBoard With { .Number = 1, .Name = "General Chat", .Filename = "general" })
                    SaveSubs()
                End If
            End SyncLock
        End Sub

        Public Sub SaveSubs()
            SyncLock _lock
                Try
                    Dim doc = New XDocument(New XElement("SubBoards"))
                    For Each sb In _subList
                        doc.Root.Add(sb.ToXml())
                    Next
                    doc.Save(_subsFile)
                Catch ex As Exception
                    Console.WriteLine($"Error saving sub-boards: {ex.Message}")
                End Try
            End SyncLock
        End Sub

        Public Function GetSubs() As List(Of SubBoard)
            Return _subList.OrderBy(Function(s) s.Number).ToList()
        End Function

        Public Function GetMessages(subNumber As Integer) As List(Of MessageHeader)
            If _msgCache.ContainsKey(subNumber) Then Return _msgCache(subNumber)
            
            Dim subBoard = _subList.FirstOrDefault(Function(s) s.Number = subNumber)
            If subBoard Is Nothing Then Return New List(Of MessageHeader)()
            
            Dim subFile = Path.Combine(_msgsDir, $"sub_{subNumber}.xml")
            If File.Exists(subFile) Then
                Try
                    Dim doc = XDocument.Load(subFile)
                    Dim msgs = New List(Of MessageHeader)()
                    For Each el In doc.Root.Elements("Message")
                        msgs.Add(MessageHeader.FromXml(el))
                    Next
                    _msgCache(subNumber) = msgs
                    Return msgs
                Catch ex As Exception
                    Console.WriteLine($"Error loading messages for sub {subNumber}: {ex.Message}")
                    Return New List(Of MessageHeader)()
                End Try
            Else
                Dim newList = New List(Of MessageHeader)()
                _msgCache(subNumber) = newList
                Return newList
            End If
        End Function

        Public Sub AddMessage(subNumber As Integer, msg As MessageHeader)
            Dim msgs = GetMessages(subNumber)
            msgs.Add(msg)
            SaveMessages(subNumber)
        End Sub

        Public Sub SaveMessages(subNumber As Integer)
            If Not _msgCache.ContainsKey(subNumber) Then Return
            
            Try
                Dim subFile = Path.Combine(_msgsDir, $"sub_{subNumber}.xml")
                Dim doc = New XDocument(New XElement("Messages"))
                For Each msg In _msgCache(subNumber)
                    doc.Root.Add(msg.ToXml())
                Next
                doc.Save(subFile)
            Catch ex As Exception
                Console.WriteLine($"Error saving messages for sub {subNumber}: {ex.Message}")
            End Try
        End Sub
    End Class
End Namespace
