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
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(1, 30, "WWIV BBS Login", TN3270Color.Yellow, TN3270Color.Blue)
            
            ' Welcome Message
            tn.WriteText(5, 10, "Welcome to WWIV Bulletin Board System")
            tn.WriteText(6, 10, "Recreated for 3270 Terminals")
            
            ' Login Fields
            tn.WriteText(10, 10, "User ID:")
            tn.AddField(10, 20, 30, "", False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "userid")
            
            tn.WriteText(12, 10, "Password:")
            Dim pwField = tn.AddField(12, 20, 30, "", False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "password")
            pwField.Intensity = TN3270Intensity.Hidden
            
            ' Instructions
            tn.WriteText(16, 10, "Press ENTER to login", TN3270Color.Yellow)
            tn.WriteText(17, 10, "Press PF3 to exit", TN3270Color.Yellow)
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, "F1=Help  F3=Exit  ENTER=Submit", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub

        Private Sub RenderTelnet(session As ISession)
            session.WriteLine("")
            session.WriteLine("╔══════════════════════════════════════════════════════════════════╗")
            session.WriteLine("║              WWIV Bulletin Board System - Telnet                ║")
            session.WriteLine("╚══════════════════════════════════════════════════════════════════╝")
            session.WriteLine("")
            session.Write("User ID: ")
        End Sub

        Public Sub HandleInput(session As ISession, input As Object) Implements IScreen.HandleInput
            If TypeOf session Is TN3270SessionAdapter Then
                HandleTN3270Input(DirectCast(session, TN3270SessionAdapter), DirectCast(input, AidKeyEventArgs))
            Else
                HandleTelnetInput(session, CStr(input))
            End If
        End Sub

        Private Sub HandleTelnetInput(session As ISession, input As String)
            If _telnetState = 0 Then
                _telnetUserId = input.Trim()
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
                    session.Write("User ID: ")
                End If
            End If
        End Sub

        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            ' Check which AID key was pressed
            Select Case e.AidKey
                Case &H7D ' ENTER key
                    Dim userId = tn.GetFieldValue("userid")
                    Dim password = tn.GetFieldValue("password")
                    
                    Console.WriteLine($"Login attempt: User={userId}, Pass={If(String.IsNullOrEmpty(password), "(empty)", "***")}")
                    
                    ' Check for NEW user registration
                    If userId?.Trim().ToUpper() = "NEW" Then
                        session.NavigateTo(New NewUserScreen())
                        Return
                    End If

                    If ValidateLogin(userId, password, session) Then
                        ' Navigate to Main Menu
                        Console.WriteLine("Login successful!")
                        session.NavigateTo(New MainMenuScreen())
                    Else
                        ' Show error and redisplay
                        tn.ClearFields()
                        RenderTN3270(session)
                        tn.WriteText(20, 10, "Invalid login. Please try again.", TN3270Color.Red)
                        tn.ShowScreen(False)
                    End If
                    
                Case &HC3 ' PF3 - Exit
                    session.Disconnect()
                    
                Case Else
                    Console.WriteLine($"Unhandled AID key: {e.AidKey:X2}")
            End Select
        End Sub

        Private Function ValidateLogin(userId As String, password As String, session As ISession) As Boolean
            ' Use UserService to find and validate user
            Dim userService As New Services.UserService()
            Dim user = userService.ValidateCredentials(userId, password)
            
            If user Is Nothing Then
                Return False
            End If
            
            ' Update session with user data
            session.User = user
            
            Console.WriteLine($"User #{user.UserNumber} ({user.Name.Trim()}) logged in successfully.")
            
            Return True
        End Function
    End Class
End Namespace
