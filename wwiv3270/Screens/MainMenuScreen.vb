Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters

Namespace WWIV.Screens
    Public Class MainMenuScreen
        Implements IScreen

        Public Sub Activate(session As ISession) Implements IScreen.Activate
            If TypeOf session Is TN3270SessionAdapter Then
                RenderTN3270(DirectCast(session, TN3270SessionAdapter))
            Else
                RenderTelnet(session)
            End If
        End Sub

        Private Sub RenderTN3270(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            tn.ClearFields() ' Always clear before re-drawing
            
            Dim userName = If(String.IsNullOrEmpty(session.User.Name), "Guest", session.User.Name.Trim())
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(1, 25, $"WWIV Main Menu - {userName}", TN3270Color.Yellow, TN3270Color.Blue)
            
            ' Menu Options
            tn.WriteText(5, 10, "WWIV Bulletin Board System", TN3270Color.Green)
            tn.WriteText(6, 10, "Main Menu", TN3270Color.Green)
            tn.WriteText(8, 10, New String("-"c, 40))
            
            tn.WriteText(10, 10, "[E] Email")
            tn.WriteText(11, 10, "[G] Goodbye (Logoff)")
            tn.WriteText(12, 10, "[M] Read Messages")
            tn.WriteText(13, 10, "[P] Post Message")
            tn.WriteText(14, 10, "[U] User List")
            tn.WriteText(15, 10, "[X] File Transfer")
            tn.WriteText(16, 10, "[Y] Your Settings")
            
            ' Sysop menu (if SL >= 100)
            If session.User.SecurityLevel >= 100 Then
                tn.WriteText(17, 10, "[//] Sysop Menu", TN3270Color.Red)
            End If
            
            tn.WriteText(19, 10, "[?] Help")
            
            ' Input Field
            tn.WriteText(20, 10, "Command:")
            tn.AddField(20, 20, 10, " ".PadRight(10), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "command")
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, $"SL:{session.User.SecurityLevel}  PF3=Logoff  ENTER=Submit", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub

        Private Sub RenderTelnet(session As ISession)
            Dim userName = If(String.IsNullOrEmpty(session.User.Name), "Guest", session.User.Name.Trim())
            session.WriteLine("")
            session.WriteLine($"╔══════════════════════════════════════════════════════════════════╗")
            session.WriteLine($"║  WWIV Main Menu - {userName.PadRight(48)} ║")
            session.WriteLine($"╚══════════════════════════════════════════════════════════════════╝")
            session.WriteLine("")
            session.WriteLine("  [E] Email")
            session.WriteLine("  [G] Goodbye (Logoff)")
            session.WriteLine("  [M] Read Messages")
            session.WriteLine("  [P] Post Message")
            session.WriteLine("  [U] User List")
            session.WriteLine("  [X] File Transfer")
            session.WriteLine("  [Y] Your Settings")
            session.WriteLine("  [?] Help")
            session.WriteLine("")
            session.Write("Command: ")
        End Sub

        Public Sub HandleInput(session As ISession, input As Object) Implements IScreen.HandleInput
            If TypeOf session Is TN3270SessionAdapter Then
                HandleTN3270Input(DirectCast(session, TN3270SessionAdapter), DirectCast(input, AidKeyEventArgs))
            End If
        End Sub

        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            Select Case e.AidKey
                Case &H7D ' ENTER
                    Dim cmd = tn.GetFieldValue("command")?.Trim().ToUpper()
                    
                    Select Case cmd
                        Case "G"
                            session.Disconnect()
                            
                        Case "//"
                            ' Sysop menu - check security level
                            If session.User.SecurityLevel >= 100 Then
                                session.NavigateTo(New SysopMenuScreen())
                            Else
                                tn.ClearFields()
                                RenderTN3270(session)
                                tn.WriteText(22, 10, "Access denied. Sysop access required.", TN3270Color.Red)
                                tn.ShowScreen(False)
                            End If
                            
                        Case "E"
                            session.NavigateTo(New LocalEmailScreen())
                            
                        Case "M", "P"
                            session.NavigateTo(New MessageBoardScreen())
                            
                        Case "U", "X", "Y", "?"
                            ' Show "Not Implemented" message
                            tn.ClearFields()
                            RenderTN3270(session)
                            tn.WriteText(22, 10, $"Command '{cmd}' not yet implemented.", TN3270Color.Yellow)
                            tn.ShowScreen(False)
                        Case Else
                            ' Invalid command
                            tn.ClearFields()
                            RenderTN3270(session)
                            tn.WriteText(22, 10, "Invalid command. Press ? for help.", TN3270Color.Red)
                            tn.ShowScreen(False)
                    End Select
                    
                Case &HC3 ' PF3 - Logoff
                    session.Disconnect()
                Case Else
                    ' Any other key, redraw to keep terminal active
                    RenderTN3270(session)
            End Select
        End Sub
    End Class
End Namespace
