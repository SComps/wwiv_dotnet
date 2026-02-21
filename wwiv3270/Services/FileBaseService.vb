Imports System
Imports System.IO
Imports System.Xml.Linq
Imports System.Collections.Concurrent
Imports System.Linq
Imports WWIV.Data

Namespace WWIV.Services
    ''' <summary>
    ''' Service for managing file directories and file information using XML
    ''' </summary>
    Public Class FileBaseService
        Private ReadOnly _dataDir As String
        Private ReadOnly _dirsFile As String
        Private ReadOnly _fileIndicesDir As String
        
        Private _dirList As List(Of FileDirectory)
        Private ReadOnly _fileCache As New ConcurrentDictionary(Of Integer, List(Of FileBaseRecord))()
        Private ReadOnly _lock As New Object()

        Public Sub New(Optional dataDir As String = Nothing)
            _dataDir = If(dataDir, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"))
            _fileIndicesDir = Path.Combine(_dataDir, "fileindices")
            _dirsFile = Path.Combine(_dataDir, "dirs.xml")
            
            If Not Directory.Exists(_fileIndicesDir) Then Directory.CreateDirectory(_fileIndicesDir)
            
            LoadDirs()
        End Sub

        Private Sub LoadDirs()
            SyncLock _lock
                If File.Exists(_dirsFile) Then
                    Try
                        Dim doc = XDocument.Load(_dirsFile)
                        _dirList = New List(Of FileDirectory)()
                        For Each el In doc.Root.Elements("FileDirectory")
                            _dirList.Add(FileDirectory.FromXml(el))
                        Next
                    Catch ex As Exception
                        Console.WriteLine($"Error loading file directories: {ex.Message}")
                        _dirList = New List(Of FileDirectory)()
                    End Try
                Else
                    _dirList = New List(Of FileDirectory)()
                    ' Add a default dir
                    _dirList.Add(New FileDirectory With { .Number = 1, .Name = "General Files", .Path = "files/general" })
                    SaveDirs()
                End If
            End SyncLock
        End Sub

        Public Sub SaveDirs()
            SyncLock _lock
                Try
                    Dim doc = New XDocument(New XElement("FileDirectories"))
                    For Each fd In _dirList
                        doc.Root.Add(fd.ToXml())
                    Next
                    doc.Save(_dirsFile)
                Catch ex As Exception
                    Console.WriteLine($"Error saving file directories: {ex.Message}")
                End Try
            End SyncLock
        End Sub

        Public Function GetDirs() As List(Of FileDirectory)
            Return _dirList.OrderBy(Function(d) d.Number).ToList()
        End Function

        Public Function GetFiles(dirNumber As Integer) As List(Of FileBaseRecord)
            If _fileCache.ContainsKey(dirNumber) Then Return _fileCache(dirNumber)
            
            Dim dir = _dirList.FirstOrDefault(Function(d) d.Number = dirNumber)
            If dir Is Nothing Then Return New List(Of FileBaseRecord)()
            
            Dim indexFile = Path.Combine(_fileIndicesDir, $"dir_{dirNumber}.xml")
            if File.Exists(indexFile) Then
                Try
                    Dim doc = XDocument.Load(indexFile)
                    Dim files = New List(Of FileBaseRecord)()
                    For Each el In doc.Root.Elements("File")
                        files.Add(FileBaseRecord.FromXml(el))
                    Next
                    _fileCache(dirNumber) = files
                    Return files
                Catch ex As Exception
                    Console.WriteLine($"Error loading files for directory {dirNumber}: {ex.Message}")
                    Return New List(Of FileBaseRecord)()
                End Try
            Else
                Dim newList = New List(Of FileBaseRecord)()
                _fileCache(dirNumber) = newList
                Return newList
            End If
        End Function

        Public Sub AddFile(dirNumber As Integer, fileRec As FileBaseRecord)
            Dim files = GetFiles(dirNumber)
            files.Add(fileRec)
            SaveFiles(dirNumber)
        End Sub

        Public Sub SaveFiles(dirNumber As Integer)
            If Not _fileCache.ContainsKey(dirNumber) Then Return
            
            Try
                Dim indexFile = Path.Combine(_fileIndicesDir, $"dir_{dirNumber}.xml")
                Dim doc = New XDocument(New XElement("Files"))
                For Each f In _fileCache(dirNumber)
                    doc.Root.Add(f.ToXml())
                Next
                doc.Save(indexFile)
            Catch ex As Exception
                Console.WriteLine($"Error saving files for directory {dirNumber}: {ex.Message}")
            End Try
        End Sub
    End Class
End Namespace
