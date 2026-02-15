Imports System
Imports System.IO
Imports System.Text.Json
Imports System.Collections.Concurrent
Imports wwiv3270.WWIV.Data

Namespace WWIV.Services
    ''' <summary>
    ''' Service for managing file directories and file information
    ''' </summary>
    Public Class FileBaseService
        Private ReadOnly _dataDir As String
        Private ReadOnly _dirsFile As String
        Private ReadOnly _fileIndicesDir As String
        
        ' Data structures are managed via WWIVJsonContext

        Private _dirList As List(Of FileDirectory)
        Private ReadOnly _fileCache As New ConcurrentDictionary(Of Integer, List(Of FileBaseRecord))()
        Private ReadOnly _lock As New Object()

        Public Sub New(Optional dataDir As String = Nothing)
            _dataDir = If(dataDir, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"))
            _fileIndicesDir = Path.Combine(_dataDir, "fileindices")
            _dirsFile = Path.Combine(_dataDir, "dirs.json")
            
            If Not Directory.Exists(_fileIndicesDir) Then Directory.CreateDirectory(_fileIndicesDir)
            
            LoadDirs()
        End Sub

        Private Sub LoadDirs()
            SyncLock _lock
                If File.Exists(_dirsFile) Then
                    Try
                        Dim json = File.ReadAllText(_dirsFile)
                        _dirList = JsonSerializer.Deserialize(json, WWIVJsonContext.Default.ListFileDirectory)
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
                    Dim json = JsonSerializer.Serialize(_dirList, WWIVJsonContext.Default.ListFileDirectory)
                    File.WriteAllText(_dirsFile, json)
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
            
            Dim indexFile = Path.Combine(_fileIndicesDir, $"dir_{dirNumber}.json")
            If File.Exists(indexFile) Then
                Try
                    Dim json = File.ReadAllText(indexFile)
                    Dim files = JsonSerializer.Deserialize(json, WWIVJsonContext.Default.ListFileBaseRecord)
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
                Dim indexFile = Path.Combine(_fileIndicesDir, $"dir_{dirNumber}.json")
                Dim json = JsonSerializer.Serialize(_fileCache(dirNumber), WWIVJsonContext.Default.ListFileBaseRecord)
                File.WriteAllText(indexFile, json)
            Catch ex As Exception
                Console.WriteLine($"Error saving files for directory {dirNumber}: {ex.Message}")
            End Try
        End Sub
    End Class
End Namespace
