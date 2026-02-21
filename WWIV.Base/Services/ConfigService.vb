Imports System
Imports System.IO
Imports System.Xml.Linq
Imports WWIV.Data

Namespace Services
    ''' <summary>
    ''' Service for managing system configuration and status using XML
    ''' </summary>
    Public Class ConfigService
        Private ReadOnly _dataDir As String
        Private ReadOnly _configFile As String
        Private ReadOnly _statusFile As String
        Private ReadOnly _menusFile As String

        Private _config As SystemConfig
        Private _status As SystemStatus
        Private _menus As List(Of MenuDefinition)
        Private Shared ReadOnly _lock As New Object()

        Public Sub New(Optional dataDir As String = Nothing)
            _dataDir = If(dataDir, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"))
            If Not Directory.Exists(_dataDir) Then Directory.CreateDirectory(_dataDir)
            
            _configFile = Path.Combine(_dataDir, "config.xml")
            _statusFile = Path.Combine(_dataDir, "status.xml")
            _menusFile = Path.Combine(_dataDir, "menus.xml")
            
            LoadConfig()
            LoadStatus()
            LoadMenus()
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

        Public Property Menus As List(Of MenuDefinition)
            Get
                SyncLock _lock
                    Return _menus
                End SyncLock
            End Get
            Set(value As List(Of MenuDefinition))
                SyncLock _lock
                    _menus = value
                End SyncLock
                SaveMenus()
            End Set
        End Property

        Private Sub LoadConfig()
            SyncLock _lock
                If File.Exists(_configFile) Then
                    Try
                        Dim doc = XDocument.Load(_configFile)
                        _config = SystemConfig.FromXml(doc.Root)
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

        Public Sub SaveConfig()
            SyncLock _lock
                Try
                    Dim doc = New XDocument(_config.ToXml())
                    doc.Save(_configFile)
                Catch ex As Exception
                    Console.WriteLine($"Error saving config: {ex.Message}")
                End Try
            End SyncLock
        End Sub

        Private Sub LoadStatus()
            SyncLock _lock
                If File.Exists(_statusFile) Then
                    Try
                        Dim doc = XDocument.Load(_statusFile)
                        _status = SystemStatus.FromXml(doc.Root)
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

        Private Sub LoadMenus()
            SyncLock _lock
                If File.Exists(_menusFile) Then
                    Try
                        Dim doc = XDocument.Load(_menusFile)
                        _menus = New List(Of MenuDefinition)()
                        For Each mdEl In doc.Root.Elements("MenuDefinition")
                            _menus.Add(MenuDefinition.FromXml(mdEl))
                        Next
                    Catch ex As Exception
                        Console.WriteLine($"Error loading menus: {ex.Message}")
                        CreateDefaultMenus()
                    End Try
                Else
                    CreateDefaultMenus()
                End If
            End SyncLock
        End Sub

        Private Sub CreateDefaultMenus()
            _menus = New List(Of MenuDefinition)()
            Dim mainMenu = New MenuDefinition With { .Id = "Main", .Title = "Main Menu" }
            mainMenu.Items.Add(New MenuItem With { .Key = "E", .Description = "Email", .Action = "Email" })
            mainMenu.Items.Add(New MenuItem With { .Key = "G", .Description = "Goodbye (Logoff)", .Action = "Logoff" })
            mainMenu.Items.Add(New MenuItem With { .Key = "M", .Description = "Read Messages", .Action = "ReadMessages" })
            mainMenu.Items.Add(New MenuItem With { .Key = "P", .Description = "Post Message", .Action = "PostMessage" })
            mainMenu.Items.Add(New MenuItem With { .Key = "U", .Description = "User List", .Action = "UserList" })
            mainMenu.Items.Add(New MenuItem With { .Key = "X", .Description = "File Transfer", .Action = "FileTransfer" })
            mainMenu.Items.Add(New MenuItem With { .Key = "Y", .Description = "Your Settings", .Action = "Settings" })
            mainMenu.Items.Add(New MenuItem With { .Key = "//", .Description = "Sysop Menu", .Action = "SysopMenu", .MinSecurityLevel = 100 })
            mainMenu.Items.Add(New MenuItem With { .Key = "?", .Description = "Help", .Action = "Help" })
            _menus.Add(mainMenu)
            SaveMenus()
        End Sub

        Public Sub SaveMenus()
            SyncLock _lock
                Try
                    Dim root = New XElement("Menus")
                    For Each md In _menus
                        root.Add(md.ToXml())
                    Next
                    Dim doc = New XDocument(root)
                    doc.Save(_menusFile)
                Catch ex As Exception
                    Console.WriteLine($"Error saving menus: {ex.Message}")
                End Try
            End SyncLock
        End Sub

        Private Sub SaveStatus()
            SyncLock _lock
                Try
                    Dim doc = New XDocument(_status.ToXml())
                    doc.Save(_statusFile)
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
