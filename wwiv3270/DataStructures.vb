Imports System

Namespace WWIV.Data
    ''' <summary>
    ''' User record - simplified for JSON storage
    ''' No marshaling needed, fully AOT-compatible
    ''' </summary>
    Public Class UserRecord
        ' Identity
        Public Property UserNumber As Integer
        Public Property Name As String = ""
        Public Property RealName As String = ""
        Public Property Password As String = ""
        
        ' Contact Information
        Public Property Phone As String = ""
        Public Property Street As String = ""
        Public Property City As String = ""
        Public Property State As String = ""
        Public Property Zipcode As String = ""
        Public Property Country As String = ""
        
        ' Demographics
        Public Property Sex As Char = "M"c
        Public Property Age As Integer
        Public Property BirthMonth As Integer
        Public Property BirthDay As Integer
        Public Property BirthYear As Integer
        
        ' Security & Access
        Public Property SecurityLevel As Integer = 10 ' SL
        Public Property DownloadSecurityLevel As Integer = 10 ' DSL
        Public Property Restrictions As Integer = 0
        Public Property IsDeleted As Boolean = False
        
        ' Activity Tracking
        Public Property FirstLogon As DateTime = DateTime.Now
        Public Property LastLogon As DateTime = DateTime.Now
        Public Property TotalLogons As Integer = 0
        Public Property LogonsToday As Integer = 0
        Public Property TimeOnToday As Single = 0
        Public Property TotalTimeOn As Single = 0
        
        ' Messages & Email
        Public Property MessagesPosted As Integer = 0
        Public Property EmailsSent As Integer = 0
        Public Property MessagesRead As Integer = 0
        Public Property PostsToday As Integer = 0
        Public Property EmailsToday As Integer = 0
        
        ' File Transfers
        Public Property FilesUploaded As Integer = 0
        Public Property FilesDownloaded As Integer = 0
        Public Property KilobytesUploaded As Long = 0
        Public Property KilobytesDownloaded As Long = 0
        
        ' Terminal Settings
        Public Property ScreenWidth As Integer = 80
        Public Property ScreenHeight As Integer = 25
        Public Property UseAnsi As Boolean = True
        Public Property UseColor As Boolean = True
        Public Property PauseOnPage As Boolean = True
        
        ' Preferences
        Public Property DefaultProtocol As String = "ZMODEM"
        Public Property DefaultEditor As String = "FULL"
        Public Property Language As String = "EN"
        
        ' System
        Public Property Note As String = "" ' Sysop notes
        Public Property Gold As Single = 0 ' Credits/points
        Public Property LastIpAddress As String = ""
        
        ' Custom Fields (extensible)
        Public Property CustomData As Dictionary(Of String, String) = New Dictionary(Of String, String)()
        
        ''' <summary>
        ''' Create a new user with default values
        ''' </summary>
        Public Shared Function CreateNew(name As String, realName As String, password As String) As UserRecord
            Return New UserRecord With {
                .Name = name,
                .RealName = realName,
                .Password = password,
                .FirstLogon = DateTime.Now,
                .LastLogon = DateTime.Now,
                .TotalLogons = 1,
                .LogonsToday = 1,
                .SecurityLevel = 10,
                .DownloadSecurityLevel = 10,
                .Age = 18,
                .BirthYear = DateTime.Now.Year - 18,
                .BirthMonth = DateTime.Now.Month,
                .BirthDay = DateTime.Now.Day
            }
        End Function
    End Class
    
    ''' <summary>
    ''' System configuration
    ''' </summary>
    Public Class SystemConfig
        Public Property SystemName As String = "WWIV BBS"
        Public Property SysopName As String = "SYSOP"
        Public Property SystemPhone As String = ""
        Public Property NewUserPassword As String = ""
        Public Property MaxUsers As Integer = 10000
        Public Property ClosedSystem As Boolean = False
        Public Property NewUserSecurityLevel As Integer = 10
        Public Property NewUserDownloadLevel As Integer = 10
        Public Property NewUserRestrictions As Integer = 0
        Public Property DataDirectory As String = "data"
        Public Property MessagesDirectory As String = "msgs"
        Public Property FilesDirectory As String = "files"
    End Class
    
    ''' <summary>
    ''' Sub-board (message area) configuration
    ''' </summary>
    Public Class SubBoard
        Public Property Number As Integer
        Public Property Name As String = ""
        Public Property Filename As String = ""
        Public Property Key As Char = " "c
        Public Property ReadSecurityLevel As Integer = 0
        Public Property PostSecurityLevel As Integer = 10
        Public Property MaxMessages As Integer = 1000
        Public Property AllowAnonymous As Boolean = False
        Public Property MinimumAge As Integer = 0
    End Class
    
    ''' <summary>
    ''' File directory configuration
    ''' </summary>
    Public Class FileDirectory
        Public Property Number As Integer
        Public Property Name As String = ""
        Public Property Path As String = ""
        Public Property DownloadSecurityLevel As Integer = 0
        Public Property UploadSecurityLevel As Integer = 10
        Public Property MaxFiles As Integer = 10000
    End Class

    ''' <summary>
    ''' System-wide status and statistics
    ''' </summary>
    Public Class SystemStatus
        Public Property DateUpdated As Date = Date.Today
        Public Property CallsToday As Integer = 0
        Public Property TotalCalls As Integer = 0
        Public Property PostsToday As Integer = 0
        Public Property TotalPosts As Integer = 0
        Public Property EmailToday As Integer = 0
        Public Property TotalEmail As Integer = 0
        Public Property FeedbackSentToday As Integer = 0
        Public Property UsersCreatedToday As Integer = 0
        Public Property TotalUsers As Integer = 0
        Public Property ActiveUsers As Integer = 0
        Public Property LocalStoreVersion As Integer = 1
    End Class

    ''' <summary>
    ''' Node/Instance monitoring information
    ''' </summary>
    Public Class InstanceRecord
        Public Property NodeNumber As Integer
        Public Property CurrentUserNumber As Integer = 0
        Public Property CurrentUserName As String = ""
        Public Property Status As String = "Waiting for caller"
        Public Property CurrentSubNumber As Integer = 0
        Public Property CurrentDirNumber As Integer = 0
        Public Property LastActivity As DateTime = DateTime.Now
        Public Property ConnectedFrom As String = ""
    End Class

    ''' <summary>
    ''' Message header - simplified for JSON
    ''' </summary>
    Public Class MessageHeader
        Public Property ID As Guid = Guid.NewGuid()
        Public Property Title As String = ""
        Public Property FromName As String = ""
        Public Property FromUserNumber As Integer = 0
        Public Property DatePosted As DateTime = DateTime.Now
        Public Property IsDeleted As Boolean = False
        Public Property IsAnonymous As Boolean = False
        Public Property IsLocked As Boolean = False
        Public Property Text As String = "" ' In JSON, we can store text inline or separately. For now, inline is simpler.
    End Class

    ''' <summary>
    ''' File record - simplified for JSON
    ''' </summary>
    Public Class FileBaseRecord
        Public Property Filename As String = ""
        Public Property Description As String = ""
        Public Property UploadedBy As String = ""
        Public Property UploadedDate As DateTime = DateTime.Now
        Public Property FileSize As Long = 0
        Public Property NumDownloads As Integer = 0
        Public Property MD5Hash As String = ""
    End Class
End Namespace
