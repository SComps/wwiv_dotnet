Imports System
Imports System.Xml.Linq
Imports System.Collections.Generic
Imports System.Linq

Namespace WWIV.Data
    ''' <summary>
    ''' User record - AOT-compatible manual XML serialization
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
        Public Property SecurityLevel As Integer = 10 
        Public Property DownloadSecurityLevel As Integer = 10 
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

        Public Function ToXml() As XElement
            Dim root = New XElement("User")
            root.Add(New XElement("UserNumber", UserNumber))
            root.Add(New XElement("Name", Name))
            root.Add(New XElement("RealName", RealName))
            root.Add(New XElement("Password", Password))
            root.Add(New XElement("Phone", Phone))
            root.Add(New XElement("Street", Street))
            root.Add(New XElement("City", City))
            root.Add(New XElement("State", State))
            root.Add(New XElement("Zipcode", Zipcode))
            root.Add(New XElement("Country", Country))
            root.Add(New XElement("Sex", Sex))
            root.Add(New XElement("Age", Age))
            root.Add(New XElement("BirthMonth", BirthMonth))
            root.Add(New XElement("BirthDay", BirthDay))
            root.Add(New XElement("BirthYear", BirthYear))
            root.Add(New XElement("SecurityLevel", SecurityLevel))
            root.Add(New XElement("DownloadSecurityLevel", DownloadSecurityLevel))
            root.Add(New XElement("Restrictions", Restrictions))
            root.Add(New XElement("IsDeleted", IsDeleted))
            root.Add(New XElement("FirstLogon", FirstLogon))
            root.Add(New XElement("LastLogon", LastLogon))
            root.Add(New XElement("TotalLogons", TotalLogons))
            root.Add(New XElement("LogonsToday", LogonsToday))
            root.Add(New XElement("TimeOnToday", TimeOnToday))
            root.Add(New XElement("TotalTimeOn", TotalTimeOn))
            root.Add(New XElement("MessagesPosted", MessagesPosted))
            root.Add(New XElement("EmailsSent", EmailsSent))
            root.Add(New XElement("MessagesRead", MessagesRead))
            root.Add(New XElement("PostsToday", PostsToday))
            root.Add(New XElement("EmailsToday", EmailsToday))
            root.Add(New XElement("FilesUploaded", FilesUploaded))
            root.Add(New XElement("FilesDownloaded", FilesDownloaded))
            root.Add(New XElement("KilobytesUploaded", KilobytesUploaded))
            root.Add(New XElement("KilobytesDownloaded", KilobytesDownloaded))
            root.Add(New XElement("ScreenWidth", ScreenWidth))
            root.Add(New XElement("ScreenHeight", ScreenHeight))
            root.Add(New XElement("UseAnsi", UseAnsi))
            root.Add(New XElement("UseColor", UseColor))
            root.Add(New XElement("PauseOnPage", PauseOnPage))
            root.Add(New XElement("DefaultProtocol", DefaultProtocol))
            root.Add(New XElement("DefaultEditor", DefaultEditor))
            root.Add(New XElement("Language", Language))
            root.Add(New XElement("Note", Note))
            root.Add(New XElement("Gold", Gold))
            root.Add(New XElement("LastIpAddress", LastIpAddress))
            
            Dim customRoot = New XElement("CustomData")
            For Each kvp In CustomData
                customRoot.Add(New XElement("Item", New XAttribute("Key", kvp.Key), kvp.Value))
            Next
            root.Add(customRoot)
            Return root
        End Function

        Public Shared Function FromXml(el As XElement) As UserRecord
            Dim u = New UserRecord()
            If el Is Nothing Then Return u
            
            u.UserNumber = CInt(el.Element("UserNumber"))
            u.Name = CStr(el.Element("Name"))
            u.RealName = CStr(el.Element("RealName"))
            u.Password = CStr(el.Element("Password"))
            u.Phone = CStr(el.Element("Phone"))
            u.Street = CStr(el.Element("Street"))
            u.City = CStr(el.Element("City"))
            u.State = CStr(el.Element("State"))
            u.Zipcode = CStr(el.Element("Zipcode"))
            u.Country = CStr(el.Element("Country"))
            u.Sex = If(CStr(el.Element("Sex"))?.Length > 0, CStr(el.Element("Sex"))(0), "M"c)
            u.Age = CInt(el.Element("Age"))
            u.BirthMonth = CInt(el.Element("BirthMonth"))
            u.BirthDay = CInt(el.Element("BirthDay"))
            u.BirthYear = CInt(el.Element("BirthYear"))
            u.SecurityLevel = CInt(el.Element("SecurityLevel"))
            u.DownloadSecurityLevel = CInt(el.Element("DownloadSecurityLevel"))
            u.Restrictions = CInt(el.Element("Restrictions"))
            u.IsDeleted = CBool(el.Element("IsDeleted"))
            u.FirstLogon = CDate(el.Element("FirstLogon"))
            u.LastLogon = CDate(el.Element("LastLogon"))
            u.TotalLogons = CInt(el.Element("TotalLogons"))
            u.LogonsToday = CInt(el.Element("LogonsToday"))
            u.TimeOnToday = CSng(el.Element("TimeOnToday"))
            u.TotalTimeOn = CSng(el.Element("TotalTimeOn"))
            u.MessagesPosted = CInt(el.Element("MessagesPosted"))
            u.EmailsSent = CInt(el.Element("EmailsSent"))
            u.MessagesRead = CInt(el.Element("MessagesRead"))
            u.PostsToday = CInt(el.Element("PostsToday"))
            u.EmailsToday = CInt(el.Element("EmailsToday"))
            u.FilesUploaded = CInt(el.Element("FilesUploaded"))
            u.FilesDownloaded = CInt(el.Element("FilesDownloaded"))
            u.KilobytesUploaded = CLng(el.Element("KilobytesUploaded"))
            u.KilobytesDownloaded = CLng(el.Element("KilobytesDownloaded"))
            u.ScreenWidth = CInt(el.Element("ScreenWidth"))
            u.ScreenHeight = CInt(el.Element("ScreenHeight"))
            u.UseAnsi = CBool(el.Element("UseAnsi"))
            u.UseColor = CBool(el.Element("UseColor"))
            u.PauseOnPage = CBool(el.Element("PauseOnPage"))
            u.DefaultProtocol = CStr(el.Element("DefaultProtocol"))
            u.DefaultEditor = CStr(el.Element("DefaultEditor"))
            u.Language = CStr(el.Element("Language"))
            u.Note = CStr(el.Element("Note"))
            u.Gold = CSng(el.Element("Gold"))
            u.LastIpAddress = CStr(el.Element("LastIpAddress"))
            
            Dim customEl = el.Element("CustomData")
            If customEl IsNot Nothing Then
                For Each item In customEl.Elements("Item")
                    Dim key = CStr(item.Attribute("Key"))
                    If key IsNot Nothing Then
                        u.CustomData(key) = CStr(item.Value)
                    End If
                Next
            End If
            
            Return u
        End Function

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

        Public Function ToXml() As XElement
            Return New XElement("Config",
                New XElement("SystemName", SystemName),
                New XElement("SysopName", SysopName),
                New XElement("SystemPhone", SystemPhone),
                New XElement("NewUserPassword", NewUserPassword),
                New XElement("MaxUsers", MaxUsers),
                New XElement("ClosedSystem", ClosedSystem),
                New XElement("NewUserSecurityLevel", NewUserSecurityLevel),
                New XElement("NewUserDownloadLevel", NewUserDownloadLevel),
                New XElement("NewUserRestrictions", NewUserRestrictions),
                New XElement("DataDirectory", DataDirectory),
                New XElement("MessagesDirectory", MessagesDirectory),
                New XElement("FilesDirectory", FilesDirectory)
            )
        End Function

        Public Shared Function FromXml(el As XElement) As SystemConfig
            Dim cfg = New SystemConfig()
            If el Is Nothing Then Return cfg
            cfg.SystemName = CStr(el.Element("SystemName"))
            cfg.SysopName = CStr(el.Element("SysopName"))
            cfg.SystemPhone = CStr(el.Element("SystemPhone"))
            cfg.NewUserPassword = CStr(el.Element("NewUserPassword"))
            cfg.MaxUsers = CInt(el.Element("MaxUsers"))
            cfg.ClosedSystem = CBool(el.Element("ClosedSystem"))
            cfg.NewUserSecurityLevel = CInt(el.Element("NewUserSecurityLevel"))
            cfg.NewUserDownloadLevel = CInt(el.Element("NewUserDownloadLevel"))
            cfg.NewUserRestrictions = CInt(el.Element("NewUserRestrictions"))
            cfg.DataDirectory = CStr(el.Element("DataDirectory"))
            cfg.MessagesDirectory = CStr(el.Element("MessagesDirectory"))
            cfg.FilesDirectory = CStr(el.Element("FilesDirectory"))
            Return cfg
        End Function
    End Class
    
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

        Public Function ToXml() As XElement
            Return New XElement("SubBoard",
                New XElement("Number", Number),
                New XElement("Name", Name),
                New XElement("Filename", Filename),
                New XElement("Key", Key),
                New XElement("ReadSecurityLevel", ReadSecurityLevel),
                New XElement("PostSecurityLevel", PostSecurityLevel),
                New XElement("MaxMessages", MaxMessages),
                New XElement("AllowAnonymous", AllowAnonymous),
                New XElement("MinimumAge", MinimumAge)
            )
        End Function

        Public Shared Function FromXml(el As XElement) As SubBoard
            Dim sb = New SubBoard()
            If el Is Nothing Then Return sb
            sb.Number = CInt(el.Element("Number"))
            sb.Name = CStr(el.Element("Name"))
            sb.Filename = CStr(el.Element("Filename"))
            sb.Key = If(CStr(el.Element("Key"))?.Length > 0, CStr(el.Element("Key"))(0), " "c)
            sb.ReadSecurityLevel = CInt(el.Element("ReadSecurityLevel"))
            sb.PostSecurityLevel = CInt(el.Element("PostSecurityLevel"))
            sb.MaxMessages = CInt(el.Element("MaxMessages"))
            sb.AllowAnonymous = CBool(el.Element("AllowAnonymous"))
            sb.MinimumAge = CInt(el.Element("MinimumAge"))
            Return sb
        End Function
    End Class
    
    Public Class FileDirectory
        Public Property Number As Integer
        Public Property Name As String = ""
        Public Property Path As String = ""
        Public Property DownloadSecurityLevel As Integer = 0
        Public Property UploadSecurityLevel As Integer = 10
        Public Property MaxFiles As Integer = 10000

        Public Function ToXml() As XElement
            Return New XElement("FileDirectory",
                New XElement("Number", Number),
                New XElement("Name", Name),
                New XElement("Path", Path),
                New XElement("DownloadSecurityLevel", DownloadSecurityLevel),
                New XElement("UploadSecurityLevel", UploadSecurityLevel),
                New XElement("MaxFiles", MaxFiles)
            )
        End Function

        Public Shared Function FromXml(el As XElement) As FileDirectory
            Dim fd = New FileDirectory()
            If el Is Nothing Then Return fd
            fd.Number = CInt(el.Element("Number"))
            fd.Name = CStr(el.Element("Name"))
            fd.Path = CStr(el.Element("Path"))
            fd.DownloadSecurityLevel = CInt(el.Element("DownloadSecurityLevel"))
            fd.UploadSecurityLevel = CInt(el.Element("UploadSecurityLevel"))
            fd.MaxFiles = CInt(el.Element("MaxFiles"))
            Return fd
        End Function
    End Class

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

        Public Function ToXml() As XElement
            Return New XElement("Status",
                New XElement("DateUpdated", DateUpdated),
                New XElement("CallsToday", CallsToday),
                New XElement("TotalCalls", TotalCalls),
                New XElement("PostsToday", PostsToday),
                New XElement("TotalPosts", TotalPosts),
                New XElement("EmailToday", EmailToday),
                New XElement("TotalEmail", TotalEmail),
                New XElement("FeedbackSentToday", FeedbackSentToday),
                New XElement("UsersCreatedToday", UsersCreatedToday),
                New XElement("TotalUsers", TotalUsers),
                New XElement("ActiveUsers", ActiveUsers),
                New XElement("LocalStoreVersion", LocalStoreVersion)
            )
        End Function

        Public Shared Function FromXml(el As XElement) As SystemStatus
            Dim ss = New SystemStatus()
            If el Is Nothing Then Return ss
            ss.DateUpdated = CDate(el.Element("DateUpdated"))
            ss.CallsToday = CInt(el.Element("CallsToday"))
            ss.TotalCalls = CInt(el.Element("TotalCalls"))
            ss.PostsToday = CInt(el.Element("PostsToday"))
            ss.TotalPosts = CInt(el.Element("TotalPosts"))
            ss.EmailToday = CInt(el.Element("EmailToday"))
            ss.TotalEmail = CInt(el.Element("TotalEmail"))
            ss.FeedbackSentToday = CInt(el.Element("FeedbackSentToday"))
            ss.UsersCreatedToday = CInt(el.Element("UsersCreatedToday"))
            ss.TotalUsers = CInt(el.Element("TotalUsers"))
            ss.ActiveUsers = CInt(el.Element("ActiveUsers"))
            ss.LocalStoreVersion = CInt(el.Element("LocalStoreVersion"))
            Return ss
        End Function
    End Class

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

    Public Class MessageHeader
        Public Property ID As Guid = Guid.NewGuid()
        Public Property Title As String = ""
        Public Property FromName As String = ""
        Public Property FromUserNumber As Integer = 0
        Public Property DatePosted As DateTime = DateTime.Now
        Public Property IsDeleted As Boolean = False
        Public Property IsAnonymous As Boolean = False
        Public Property IsLocked As Boolean = False
        Public Property Text As String = ""

        Public Overridable Function ToXml() As XElement
            Return New XElement("Message",
                New XAttribute("Type", Me.GetType().Name),
                New XElement("ID", ID),
                New XElement("Title", Title),
                New XElement("FromName", FromName),
                New XElement("FromUserNumber", FromUserNumber),
                New XElement("DatePosted", DatePosted),
                New XElement("IsDeleted", IsDeleted),
                New XElement("IsAnonymous", IsAnonymous),
                New XElement("IsLocked", IsLocked),
                New XElement("Text", Text)
            )
        End Function

        Public Shared Function FromXml(el As XElement) As MessageHeader
            Dim typeName = CStr(el.Attribute("Type"))
            Dim m As MessageHeader
            If typeName = "EmailMessage" Then
                m = New EmailMessage()
            Else
                m = New MessageHeader()
            End If
            
            m.ID = Guid.Parse(CStr(el.Element("ID")))
            m.Title = CStr(el.Element("Title"))
            m.FromName = CStr(el.Element("FromName"))
            m.FromUserNumber = CInt(el.Element("FromUserNumber"))
            m.DatePosted = CDate(el.Element("DatePosted"))
            m.IsDeleted = CBool(el.Element("IsDeleted"))
            m.IsAnonymous = CBool(el.Element("IsAnonymous"))
            m.IsLocked = CBool(el.Element("IsLocked"))
            m.Text = CStr(el.Element("Text"))
            
            If TypeOf m Is EmailMessage Then
                DirectCast(m, EmailMessage).ToUserNumber = CInt(el.Element("ToUserNumber"))
                DirectCast(m, EmailMessage).ToName = CStr(el.Element("ToName"))
                DirectCast(m, EmailMessage).IsRead = CBool(el.Element("IsRead"))
            End If
            
            Return m
        End Function
    End Class

    Public Class FileBaseRecord
        Public Property Filename As String = ""
        Public Property Description As String = ""
        Public Property UploadedBy As String = ""
        Public Property UploadedDate As DateTime = DateTime.Now
        Public Property FileSize As Long = 0
        Public Property NumDownloads As Integer = 0
        Public Property MD5Hash As String = ""

        Public Function ToXml() As XElement
            Return New XElement("File",
                New XElement("Filename", Filename),
                New XElement("Description", Description),
                New XElement("UploadedBy", UploadedBy),
                New XElement("UploadedDate", UploadedDate),
                New XElement("FileSize", FileSize),
                New XElement("NumDownloads", NumDownloads),
                New XElement("MD5Hash", MD5Hash)
            )
        End Function

        Public Shared Function FromXml(el As XElement) As FileBaseRecord
            Dim f = New FileBaseRecord()
            If el Is Nothing Then Return f
            f.Filename = CStr(el.Element("Filename"))
            f.Description = CStr(el.Element("Description"))
            f.UploadedBy = CStr(el.Element("UploadedBy"))
            f.UploadedDate = CDate(el.Element("UploadedDate"))
            f.FileSize = CLng(el.Element("FileSize"))
            f.NumDownloads = CInt(el.Element("NumDownloads"))
            f.MD5Hash = CStr(el.Element("MD5Hash"))
            Return f
        End Function
    End Class

    Public Class EmailMessage
        Inherits MessageHeader
        Public Property ToUserNumber As Integer = 0
        Public Property ToName As String = ""
        Public Property IsRead As Boolean = False

        Public Overrides Function ToXml() As XElement
            Dim root = MyBase.ToXml()
            root.Add(New XElement("ToUserNumber", ToUserNumber))
            root.Add(New XElement("ToName", ToName))
            root.Add(New XElement("IsRead", IsRead))
            Return root
        End Function
    End Class
End Namespace
