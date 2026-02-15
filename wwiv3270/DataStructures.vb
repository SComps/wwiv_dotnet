Imports System
Imports System.Runtime.InteropServices

Namespace WWIV.Data
    ' Constants and Enums
    Public Class Constants
        Public Const OK_LEVEL As Integer = 0
        Public Const NOK_LEVEL As Integer = 1
        Public Const QUIT_LEVEL As Integer = 2
        Public Const EVENT_LEVEL As Integer = 4

        Public Const DELIMS_NORMAL As String = " ;.!:-?," & vbTab & vbCr & vbLf
        Public Const DELIMS_WHITE As String = " " & vbTab & vbCr & vbLf

        ' Instance Status Flags
        Public Const INST_FLAGS_NONE As UShort = &H0
        Public Const INST_FLAGS_ONLINE As UShort = &H1
        Public Const INST_FLAGS_MSG_AVAIL As UShort = &H2
        Public Const INST_FLAGS_NOCHANGE As UShort = &H8000

        ' Instance Locations
        Public Const INST_LOC_DOWN As UShort = 0
        Public Const INST_LOC_INIT As UShort = 1
        Public Const INST_LOC_EMAIL As UShort = 2
        Public Const INST_LOC_MAIN As UShort = 3
        Public Const INST_LOC_XFER As UShort = 4
        Public Const INST_LOC_CHAINS As UShort = 5
        Public Const INST_LOC_NET As UShort = 6
        Public Const INST_LOC_GFILES As UShort = 7
        Public Const INST_LOC_BEGINDAY As UShort = 8
        Public Const INST_LOC_EVENT As UShort = 9
        Public Const INST_LOC_CHAT As UShort = 10
        Public Const INST_LOC_CHAT2 As UShort = 11
        Public Const INST_LOC_CHATROOM As UShort = 12
        Public Const INST_LOC_LOGON As UShort = 13
        Public Const INST_LOC_LOGOFF As UShort = 14
        Public Const INST_LOC_FSED As UShort = 15
        Public Const INST_LOC_UEDIT As UShort = 16
        Public Const INST_LOC_CHAINEDIT As UShort = 17
        Public Const INST_LOC_BOARDEDIT As UShort = 18
        Public Const INST_LOC_DIREDIT As UShort = 19
        Public Const INST_LOC_GFILEEDIT As UShort = 20
        Public Const INST_LOC_CONFEDIT As UShort = 21
        Public Const INST_LOC_DOS As UShort = 22
        Public Const INST_LOC_DEFAULTS As UShort = 23
        Public Const INST_LOC_REBOOT As UShort = 24
        Public Const INST_LOC_RELOAD As UShort = 25
        Public Const INST_LOC_VOTE As UShort = 26
        Public Const INST_LOC_BANK As UShort = 27
        Public Const INST_LOC_AMSG As UShort = 28
        Public Const INST_LOC_SUBS As UShort = 29
        Public Const INST_LOC_CHUSER As UShort = 30
        Public Const INST_LOC_TEDIT As UShort = 31
        Public Const INST_LOC_MAILR As UShort = 32
        Public Const INST_LOC_RESETQSCAN As UShort = 33
        Public Const INST_LOC_VOTEEDIT As UShort = 34
        Public Const INST_LOC_VOTEPRINT As UShort = 35
        Public Const INST_LOC_RESETF As UShort = 36
        Public Const INST_LOC_FEEDBACK As UShort = 37
        Public Const INST_LOC_KILLEMAIL As UShort = 38
        Public Const INST_LOC_POST As UShort = 39
        Public Const INST_LOC_NEWUSER As UShort = 40
        Public Const INST_LOC_RMAIL As UShort = 41
        Public Const INST_LOC_DOWNLOAD As UShort = 42
        Public Const INST_LOC_UPLOAD As UShort = 43
        Public Const INST_LOC_BIXFER As UShort = 44
        Public Const INST_LOC_NETLIST As UShort = 45
        Public Const INST_LOC_WFC As UShort = 65535
    End Class

    <StructLayout(LayoutKind.Sequential, Pack:=1, CharSet:=CharSet.Ansi)>
    Public Structure InstanceRec
        Public Number As Short
        Public User As Short
        Public Flags As UShort
        Public Loc As UShort
        Public SubLoc As UShort
        Public LastUpdate As UInteger
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=86)>
        Public Extra As String
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1, CharSet:=CharSet.Ansi)>
    Public Structure UserRec
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=31)> Public Name As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public RealName As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=7)> Public CallSign As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=13)> Public Phone As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=13)> Public DataPhone As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=31)> Public Street As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=31)> Public City As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=3)> Public State As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=4)> Public Country As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=11)> Public Zipcode As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=9)> Public Pw As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=9)> Public LastOn As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=9)> Public FirstOn As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=61)> Public Note As String
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=3 * 81)> Public Macros As Byte() ' Flattened or handle manually
        Public Sex As Byte
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=78)> Public ResChar As Byte()
        Public Age As Byte
        Public Inact As Byte
        Public CompType As Byte
        Public DefProt As Byte
        Public DefEd As Byte
        Public ScreenChars As Byte
        Public ScreenLines As Byte
        Public NumExtended As Byte
        Public OptionalVal As Byte
        Public Sl As Byte
        Public Dsl As Byte
        Public Exempt As Byte
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=10)> Public Colors As Byte()
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=10)> Public BwColors As Byte()
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=20)> Public Votes As Byte()
        Public Illegal As Byte
        Public Waiting As Byte
        Public OnToday As Byte
        Public Month As Byte
        Public Day As Byte
        Public Year As Byte
        Public Language As Byte
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=50)> Public ResByte As Byte()
        Public HomeUser As UShort
        Public HomeSys As UShort
        Public ForwardUsr As UShort
        Public ForwardSys As UShort
        Public NetNum As UShort
        Public MsgPost As UShort
        Public EmailSent As UShort
        Public FeedbackSent As UShort
        Public FSentToday1 As UShort
        Public PostToday As UShort
        Public EToday As UShort
        Public Ar As UShort
        Public Dar As UShort
        Public Restrict As UShort
        Public AssPts As UShort
        Public Uploaded As UShort
        Public Downloaded As UShort
        Public LastRate As UShort
        Public Logons As UShort
        Public EmailNet As UShort
        Public PostNet As UShort
        Public DeletedPosts As UShort
        Public ChainsRun As UShort
        Public GFilesRead As UShort
        Public BankTime As UShort
        Public HomeNet As UShort
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=48)> Public ResShort As Byte()
        Public MsgRead As UInteger
        Public Uk As UInteger
        Public Dk As UInteger
        Public Daten As UInteger
        Public SysStatus As UInteger
        Public WwivRegNum As UInteger
        Public FilePoints As UInteger
        Public Registered As UInteger
        Public Expires As UInteger
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=48)> Public ResLong As Byte()
        Public TimeOnToday As Single
        Public ExtraTime As Single
        Public TimeOn As Single
        Public PosAccount As Single
        Public NegAccount As Single
        Public Gold As Single
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=32)> Public ResFloat As Byte()
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=100)> Public ResGp As Byte()
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1, CharSet:=CharSet.Ansi)>
    Public Structure SlRec
        Public TimePerDay As UShort
        Public TimePerLogon As UShort
        Public MessagesRead As UShort
        Public Emails As UShort
        Public Posts As UShort
        Public Ability As UInteger
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1, CharSet:=CharSet.Ansi)>
    Public Structure ValRec
        Public Sl As Byte
        Public Dsl As Byte
        Public Ar As UShort
        Public Dar As UShort
        Public Restrict As UShort
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1, CharSet:=CharSet.Ansi)>
    Public Structure ConfigRec
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public NewUserPw As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public SystemPw As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=81)> Public MsgsDir As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=81)> Public GFilesDir As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=81)> Public DataDir As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=81)> Public DLoadsDir As String
        Public RamDrive As Byte
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=81)> Public TempDir As String
        Public XMark As Byte
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=83)> Public RegCode As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=51)> Public BbsInitModem As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Answer As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Connect300 As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Connect1200 As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Connect2400 As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Connect9600 As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Connect19200 As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public NoCarrier As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Ring As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Terminal As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=51)> Public SystemName As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=13)> Public SystemPhone As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=51)> Public SysopName As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=51)> Public ExecuteStr As String
        Public NewUserSl As Byte
        Public NewUserDsl As Byte
        Public MaxWaiting As Byte
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=5)> Public ComPort As Byte()
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=5)> Public ComIsr As Byte()
        Public PrimaryPort As Byte
        Public NewUploads As Byte
        Public ClosedSystem As Byte
        Public SystemNumber As UShort
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=5)> Public BaudRate As UShort()
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=5)> Public ComBase As UShort()
        Public MaxUsers As UShort
        Public NewUserRestrict As UShort
        Public SysConfig As UShort
        Public SysopLowTime As UShort
        Public SysopHighTime As UShort
        Public ExecuteTime As UShort
        Public ReqRatio As Single
        Public NewUserGold As Single
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=256)> Public Sl As SlRec()
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=10)> Public AutoVal As ValRec()
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public HangupPhone As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public PickupPhone As String
        Public NetLowTime As UShort
        Public NetHighTime As UShort
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Connect300A As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Connect1200A As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Connect2400A As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Connect9600A As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public Connect19200A As String
        ' TODO: ArcRecs
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=4 * (4 + 32 + 32 + 32))> Public Arcs As Byte() 
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=51)> Public BeginDayC As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=51)> Public LogonC As String
        Public UserRecLen As Short
        Public WaitingOffset As Short
        Public InactOffset As Short
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=51)> Public NewUserC As String
        Public WwivRegNumber As UInteger
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=21)> Public DialPrefix As String
        Public PostCallRatio As Single
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=51)> Public UploadC As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=81)> Public DszBatchDl As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=9)> Public ModemType As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=81)> Public BatchDir As String
        Public SysStatusOffset As Short
        Public NetworkType As Byte
        Public FuOffset As Short
        Public FsOffset As Short
        Public FnOffset As Short
        Public MaxSubs As UShort
        Public MaxDirs As UShort
        Public QscnLen As UShort
        Public EmailStorageType As Byte
        Public SysConfig1 As UInteger
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=19)> Public Res As Byte()
    End Structure
    
    <StructLayout(LayoutKind.Sequential, Pack:=1, CharSet:=CharSet.Ansi)>
    Public Structure SubBoardRec
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=41)> Public Name As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=9)> Public Filename As String
        Public Key As Byte
        Public ReadSl As Byte
        Public PostSl As Byte
        Public Anony As Byte
        Public Age As Byte
        Public MaxMsgs As UShort
        Public Ar As UShort
        Public StorageType As UShort
        Public Type As UShort
    End Structure
End Namespace
