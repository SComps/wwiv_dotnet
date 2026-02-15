Imports System
Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports wwiv3270.WWIV.Data

Namespace WWIV.Services
    ''' <summary>
    ''' Service for managing system configuration and status
    ''' </summary>
    Public Class ConfigService
        Private ReadOnly _dataDir As String
        Private ReadOnly _configFile As String
        Private ReadOnly _statusFile As String
        ' Data structures are managed via WWIVJsonContext

        Private _config As SystemConfig
        Private _status As SystemStatus
        Private Shared ReadOnly _lock As New Object()

        Public Sub New(Optional dataDir As String = Nothing)
            _dataDir = If(dataDir, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"))
            If Not Directory.Exists(_dataDir) Then Directory.CreateDirectory(_dataDir)
            
            _configFile = Path.Combine(_dataDir, "config.json")
            _statusFile = Path.Combine(_dataDir, "status.json")
            
            LoadConfig()
            LoadStatus()
        End Sub

        Public Property Config As SystemConfig
            Get
                SyncLock _lock
                    Return _config
                End SyncLock
            End Get
            Set(value As SystemConfig)
                SyncLock _lock
                    _config = value
                End SyncLock
                SaveConfig()
            End Set
        End Property

        Public Property Status As SystemStatus
            Get
                SyncLock _lock
                    Return _status
                End SyncLock
            End Get
            Set(value As SystemStatus)
                SyncLock _lock
                    _status = value
                End SyncLock
                SaveStatus()
            End Set
        End Property

        Private Sub LoadConfig()
            SyncLock _lock
                If File.Exists(_configFile) Then
                    Try
                        Dim json = File.ReadAllText(_configFile)
                        _config = JsonSerializer.Deserialize(json, WWIVJsonContext.Default.SystemConfig)
                    Catch ex As Exception
                        Console.WriteLine($"Error loading config: {ex.Message}")
                        _config = New SystemConfig()
                    End Try
                Else
                    _config = New SystemConfig()
                    SaveConfig()
                End If
            End SyncLock
        End Sub

        Private Sub SaveConfig()
            SyncLock _lock
                Try
                    Dim json = JsonSerializer.Serialize(_config, WWIVJsonContext.Default.SystemConfig)
                    File.WriteAllText(_configFile, json)
                Catch ex As Exception
                    Console.WriteLine($"Error saving config: {ex.Message}")
                End Try
            End SyncLock
        End Sub

        Private Sub LoadStatus()
            SyncLock _lock
                If File.Exists(_statusFile) Then
                    Try
                        Dim json = File.ReadAllText(_statusFile)
                        _status = JsonSerializer.Deserialize(json, WWIVJsonContext.Default.SystemStatus)
                    Catch ex As Exception
                        Console.WriteLine($"Error loading status: {ex.Message}")
                        _status = New SystemStatus()
                    End Try
                Else
                    _status = New SystemStatus()
                    SaveStatus()
                End If
                
                ' Check if we need to reset daily stats
                If _status.DateUpdated.Date <> DateTime.Today Then
                    ResetDailyStats()
                End If
            End SyncLock
        End Sub

        Private Sub SaveStatus()
            SyncLock _lock
                Try
                    Dim json = JsonSerializer.Serialize(_status, WWIVJsonContext.Default.SystemStatus)
                    File.WriteAllText(_statusFile, json)
                Catch ex As Exception
                    Console.WriteLine($"Error saving status: {ex.Message}")
                End Try
            End SyncLock
        End Sub

        Private Sub ResetDailyStats()
            _status.CallsToday = 0
            _status.PostsToday = 0
            _status.EmailToday = 0
            _status.FeedbackSentToday = 0
            _status.UsersCreatedToday = 0
            _status.DateUpdated = DateTime.Today
            SaveStatus()
        End Sub

        ''' <summary>
        ''' Increment global system calls
        ''' </summary>
        Public Sub IncrementCalls()
            SyncLock _lock
                If _status.DateUpdated.Date <> DateTime.Today Then ResetDailyStats()
                _status.CallsToday += 1
                _status.TotalCalls += 1
                SaveStatus()
            End SyncLock
        End Sub
    End Class
End Namespace
