Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters

Namespace WWIV.Screens
    Public Class LoginScreen
        Implements IScreen

        Private _telnetUserId As String = ""
        Private _telnetState As Integer = 0 ' 0=UserId, 1=Password

        Public Sub Activate(session As ISession) Implements IScreen.Activate
            ' Check if this is a TN3270 session or Telnet
            If TypeOf session Is TN3270SessionAdapter Then
                RenderTN3270(DirectCast(session, TN3270SessionAdapter))
            Else
                RenderTelnet(session)
            End If
        End Sub

        Private Sub RenderTN3270(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            tn.ClearFields()
            Dim configService As New Global.WWIV.Services.ConfigService()
            Dim bbsName = configService.Config.SystemName
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(1, 30, $"{bbsName} Login", TN3270Color.Yellow, TN3270Color.Blue)
            
            ' Welcome Message
            tn.WriteText(5, 10, $"Welcome to {bbsName}")
            tn.WriteText(6, 10, "Recreated for 3270 Terminals")
            
            ' Login Fields
            tn.WriteText(10, 10, "User ID:")
            tn.AddField(10, 20, 30, " ".PadRight(30), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "userid")
            
            tn.WriteText(12, 10, "Password:")
            Dim pwField = tn.AddField(12, 20, 30, " ".PadRight(30), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "password")
            pwField.Intensity = TN3270Intensity.Hidden
            
            ' Instructions
            tn.WriteText(16, 10, "Type 'NEW' for a new account.", TN3270Color.Turquoise)
            tn.WriteText(18, 10, "Press ENTER to login", TN3270Color.Yellow)
            tn.WriteText(19, 10, "Press PF3 to exit", TN3270Color.Yellow)
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, "F1=Help  F3=Exit  ENTER=Submit", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub

        Private Sub RenderTelnet(session As ISession)
            Dim configService As New Global.WWIV.Services.ConfigService()
            Dim bbsName = configService.Config.SystemName
            session.ClearScreen()
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Cyan, Util.Ansi.BgBlue) & $" {bbsName} ".PadRight(70) & Util.Ansi.Reset)
            session.WriteLine("")
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Yellow) & "Welcome to a New Generation of WWIV!" & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "--------------------------------------------------------------------" & Util.Ansi.Reset)
            session.WriteLine("")
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Green) & "(Type 'NEW' for a new account)" & Util.Ansi.Reset)
            session.WriteLine("")
            session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "User ID: " & Util.Ansi.Reset)
        End Sub

        Public Sub HandleInput(session As ISession, input As Object) Implements IScreen.HandleInput
            If TypeOf session Is TN3270SessionAdapter Then
                HandleTN3270Input(DirectCast(session, TN3270SessionAdapter), DirectCast(input, AidKeyEventArgs))
            ElseIf TypeOf input Is String Then
                HandleTelnetInput(session, DirectCast(input, String))
            End If
        End Sub

        Private Sub HandleTelnetInput(session As ISession, input As String)
            If _telnetState = 0 Then
                _telnetUserId = input.Trim()
                If String.IsNullOrEmpty(_telnetUserId) Then
                    RenderTelnet(session)
                    Return
                End If
                If _telnetUserId.ToUpper() = "NEW" Then
                    session.NavigateTo(New NewUserScreen())
                    Return
                End If
                _telnetState = 1
                session.Write("Password: ")
            Else
                Dim password = input.Trim()
                If ValidateLogin(_telnetUserId, password, session) Then
                    session.WriteLine("Login successful!")
                    session.NavigateTo(New MainMenuScreen())
                Else
                    session.WriteLine("Invalid login.")
                    _telnetState = 0
                    RenderTelnet(session)
                End If
            End If
        End Sub

        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            ' Check which AID key was pressed
            Select Case e.AidKey
                Case AID.ENTER ' ENTER key
                    Dim userId = tn.GetFieldValue("userid")?.Trim()
                    Dim password = tn.GetFieldValue("password")?.Trim()
                    
                    ' Check for NEW user registration
                    If userId?.ToUpper() = "NEW" Then
                        session.NavigateTo(New NewUserScreen())
                        Return
                    End If

                    If ValidateLogin(userId, password, session) Then
                        session.NavigateTo(New MainMenuScreen())
                    Else
                        ' Show error and redisplay
                        RenderTN3270(session)
                        tn.WriteText(22, 10, "Invalid login. Please try again.", TN3270Color.Red)
                        tn.ShowScreen(False)
                    End If
                    
                Case AID.PF3 ' PF3 - Exit
                    session.Disconnect()
                    
                Case Else
                    RenderTN3270(session)
            End Select
        End Sub

        Private Function ValidateLogin(userId As String, password As String, session As ISession) As Boolean
            If String.IsNullOrEmpty(userId) Then Return False
            
            Dim userService As New Global.WWIV.Services.UserService()
            Dim user = userService.ValidateCredentials(userId, password)
            
            If user Is Nothing Then
                Return False
            End If
            
            session.User = user
            Return True
        End Function
    End Class
End Namespace
