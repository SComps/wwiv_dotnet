Imports System
Imports System.IO
Imports System.Xml.Linq
Imports System.Collections.Generic
Imports System.Linq
Imports WWIV.Data

Namespace Services
    ''' <summary>
    ''' Service for managing user accounts using XML storage
    ''' AOT-compatible manual serialization
    ''' </summary>
    Public Class UserService
        Private ReadOnly _dataDir As String
        Private ReadOnly _usersFile As String
        
        ' In-memory cache for performance
        Private _userCache As Dictionary(Of Integer, UserRecord)
        Private _nameIndex As Dictionary(Of String, Integer)
        Private _nextUserNumber As Integer = 1
        
        Public Sub New(Optional dataDir As String = Nothing)
            _dataDir = If(dataDir, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"))
            _usersFile = Path.Combine(_dataDir, "users.xml")
            
            ' Ensure data directory exists
            If Not Directory.Exists(_dataDir) Then
                Directory.CreateDirectory(_dataDir)
            End If
            
            ' Load users into cache
            LoadUsers()
        End Sub
        
        Private Sub LoadUsers()
            _userCache = New Dictionary(Of Integer, UserRecord)()
            _nameIndex = New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            
            If Not File.Exists(_usersFile) Then
                SaveUsers()
                Return
            End If
            
            Try
                Dim doc = XDocument.Load(_usersFile)
                Dim usersRoot = doc.Element("Users")
                
                If usersRoot IsNot Nothing Then
                    For Each userEl In usersRoot.Elements("User")
                        Dim user = UserRecord.FromXml(userEl)
                        If Not user.IsDeleted Then
                            _userCache(user.UserNumber) = user
                            If Not String.IsNullOrEmpty(user.Name) Then
                                _nameIndex(user.Name.Trim()) = user.UserNumber
                            End If
                            
                            If user.UserNumber >= _nextUserNumber Then
                                _nextUserNumber = user.UserNumber + 1
                            End If
                        End If
                    Next
                End If
                
                Console.WriteLine($"Loaded {_userCache.Count} users from {_usersFile}")
            Catch ex As Exception
                Console.WriteLine($"Error loading users: {ex.Message}")
                _userCache.Clear()
                _nameIndex.Clear()
            End Try
        End Sub
        
        Private Sub SaveUsers()
            Try
                Dim doc = New XDocument(New XElement("Users"))
                For Each user In _userCache.Values
                    doc.Root.Add(user.ToXml())
                Next
                doc.Save(_usersFile)
            Catch ex As Exception
                Console.WriteLine($"Error saving users: {ex.Message}")
            End Try
        End Sub
        
        Public Function GetUser(userNumber As Integer) As UserRecord
            If _userCache.ContainsKey(userNumber) Then
                Return _userCache(userNumber)
            End If
            Return Nothing
        End Function
        
        Public Function FindUser(name As String) As Integer
            If String.IsNullOrWhiteSpace(name) Then Return 0
            
            Dim searchName = name.Trim()
            
            ' Special case for new user
            If searchName.Equals("NEW", StringComparison.OrdinalIgnoreCase) Then
                Return -1
            End If
            
            ' 1. Check name index FIRST (case-insensitive)
            If _nameIndex.ContainsKey(searchName) Then
                Return _nameIndex(searchName)
            End If

            ' 2. Try numeric lookup if not found by name
            Dim userNum As Integer
            If Integer.TryParse(searchName, userNum) Then
                If _userCache.ContainsKey(userNum) Then
                    Return userNum
                End If
                Return 0
            End If
            
            Return 0
        End Function
        
        Public Sub SaveUser(user As UserRecord)
            If user Is Nothing Then Return
            
            ' Assign user number if new
            If user.UserNumber = 0 Then
                ' User #1 is the Master Sysop
                If _nextUserNumber = 1 Then
                    user.SecurityLevel = 255
                    user.DownloadSecurityLevel = 255
                    Console.WriteLine("Assigning Master Sysop status to User #1")
                End If
                
                user.UserNumber = _nextUserNumber
                _nextUserNumber += 1
            End If
            
            ' Update cache
            _userCache(user.UserNumber) = user
            If Not String.IsNullOrEmpty(user.Name) Then
                _nameIndex(user.Name.Trim()) = user.UserNumber
            End If
            
            ' Persist to disk
            SaveUsers()
            
            Console.WriteLine($"Saved user #{user.UserNumber}: {user.Name.Trim()}")
        End Sub
        
        Public Function CreateUser(name As String, realName As String, password As String) As UserRecord
            If FindUser(name) > 0 Then
                Throw New InvalidOperationException($"User '{name}' already exists")
            End If
            
            Dim user = UserRecord.CreateNew(name, realName, password)
            SaveUser(user)
            Return user
        End Function
        
        Public Sub DeleteUser(userNumber As Integer)
            If Not _userCache.ContainsKey(userNumber) Then Return
            
            Dim user = _userCache(userNumber)
            user.IsDeleted = True
            
            ' Remove from cache and index
            _userCache.Remove(userNumber)
            _nameIndex.Remove(user.Name.Trim())
            
            SaveUsers()
            
            Console.WriteLine($"Deleted user #{userNumber}: {user.Name.Trim()}")
        End Sub
        
        Public Function GetAllUsers() As List(Of UserRecord)
            Return _userCache.Values.OrderBy(Function(u) u.UserNumber).ToList()
        End Function
        
        Public Function GetUserCount() As Integer
            Return _userCache.Count
        End Function
        
        Public Function ValidateCredentials(username As String, password As String) As UserRecord
            Console.WriteLine($"Validating credentials for '{username}' (Len:{username?.Length})")
            Dim userNum = FindUser(username)
            If userNum <= 0 Then 
                Console.WriteLine($"User '{username}' not found.")
                Return Nothing
            End If
            
            Dim user = GetUser(userNum)
            If user Is Nothing Then Return Nothing
            
            Console.WriteLine($"Checking password for user #{userNum} ({user.Name.Trim()}). Provided Len:{password?.Length}, Stored Len:{user.Password?.Trim().Length}")
            
            If String.Equals(user.Password.Trim(), password.Trim(), StringComparison.OrdinalIgnoreCase) Then
                ' Update last logon
                user.LastLogon = DateTime.Now
                user.TotalLogons += 1
                user.LogonsToday += 1
                SaveUser(user)
                Return user
            Else
                Console.WriteLine("Password mismatch.")
            End If
            
            Return Nothing
        End Function
        
        Public Sub UpdateActivity(userNumber As Integer, timeSpent As Single)
            Dim user = GetUser(userNumber)
            If user Is Nothing Then Return
            
            user.LastLogon = DateTime.Now
            user.TimeOnToday += timeSpent
            user.TotalTimeOn += timeSpent
            SaveUser(user)
        End Sub
    End Class
End Namespace
