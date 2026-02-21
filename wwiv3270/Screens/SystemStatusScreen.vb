Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports WWIV.Data
Imports WWIV.Services

Namespace WWIV.Screens
    ''' <summary>
    ''' System Status Screen
    ''' Displays BBS statistics and current state
    ''' </summary>
    Public Class SystemStatusScreen
        Implements IScreen
        
        Private _configService As ConfigService
        
        Public Sub New()
            _configService = New ConfigService()
        End Sub
        
        Public Sub Activate(session As ISession) Implements IScreen.Activate
            If TypeOf session Is TN3270SessionAdapter Then
                RenderTN3270(DirectCast(session, TN3270SessionAdapter))
            Else
                RenderTelnet(session)
            End If
        End Sub
        
        Private Sub RenderTN3270(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            tn.ClearFields()
            
            Dim status = _configService.Status
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.Red, TN3270Color.Neutral)
            tn.WriteText(1, 30, "WWIV System Status", TN3270Color.Yellow, TN3270Color.Red)
            
            ' Statistics
            tn.WriteText(4, 10, "Calls Today      :", TN3270Color.Turquoise)
            tn.WriteText(4, 30, status.CallsToday.ToString(), TN3270Color.Green)
            
            tn.WriteText(5, 10, "Total Calls      :", TN3270Color.Turquoise)
            tn.WriteText(5, 30, status.TotalCalls.ToString(), TN3270Color.Green)
            
            tn.WriteText(7, 10, "Posts Today      :", TN3270Color.Turquoise)
            tn.WriteText(7, 30, status.PostsToday.ToString(), TN3270Color.White)
            
            tn.WriteText(8, 10, "Total Posts      :", TN3270Color.Turquoise)
            tn.WriteText(8, 30, status.TotalPosts.ToString(), TN3270Color.White)
            
            tn.WriteText(10, 10, "Emails Today     :", TN3270Color.Turquoise)
            tn.WriteText(10, 30, status.EmailToday.ToString(), TN3270Color.Yellow)
            
            tn.WriteText(11, 10, "Total Emails     :", TN3270Color.Turquoise)
            tn.WriteText(11, 30, status.TotalEmail.ToString(), TN3270Color.Yellow)
            
            tn.WriteText(13, 10, "Active Users     :", TN3270Color.Turquoise)
            tn.WriteText(13, 30, status.ActiveUsers.ToString(), TN3270Color.Turquoise)
            
            tn.WriteText(14, 10, "Total Users      :", TN3270Color.Turquoise)
            tn.WriteText(14, 30, status.TotalUsers.ToString(), TN3270Color.Turquoise)
            
            tn.WriteText(16, 10, "System Date      :", TN3270Color.Turquoise)
            tn.WriteText(16, 30, status.DateUpdated.ToLongDateString(), TN3270Color.Pink)
            
            ' Prompt
            tn.WriteText(20, 10, "Press PF3 or ENTER to Return", TN3270Color.Turquoise)
            
            ' Command field (invisible or small for consistency)
            tn.AddField(22, 1, 1, " ", False, TN3270Color.Neutral, TN3270Color.Neutral, TN3270Highlight.None, "dummy")
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, "System Information Display", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub

        Private Sub RenderTelnet(session As ISession)
            Dim status = _configService.Status
            session.ClearScreen()
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.BgRed) & " WWIV System Status ".PadRight(70) & Util.Ansi.Reset)
            session.WriteLine("")
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "Calls Today   : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & status.CallsToday & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "Total Calls   : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & status.TotalCalls & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "Posts Today   : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.White) & status.PostsToday & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "Total Posts   : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.White) & status.TotalPosts & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "Emails Today  : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Yellow) & status.EmailToday & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "Total Emails  : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Yellow) & status.TotalEmail & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "Active Users  : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Turquoise) & status.ActiveUsers & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "Total Users   : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Turquoise) & status.TotalUsers & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "System Date   : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Magenta) & status.DateUpdated.ToLongDateString() & Util.Ansi.Reset)
            session.WriteLine("")
            session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "(Press ENTER to Return)" & Util.Ansi.Reset)
        End Sub
        
        Public Sub HandleInput(session As ISession, input As Object) Implements IScreen.HandleInput
            session.NavigateTo(New SysopMenuScreen())
        End Sub
    End Class
End Namespace
