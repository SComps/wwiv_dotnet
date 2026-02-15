Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters

Namespace WWIV.Screens
    ''' <summary>
    ''' Sysop Menu Screen
    ''' Provides access to system administration functions
    ''' </summary>
    Public Class SysopMenuScreen
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
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.Red, TN3270Color.Neutral)
            tn.WriteText(1, 28, "WWIV Sysop Menu", TN3270Color.Yellow, TN3270Color.Red)
            
            ' Menu Options
            tn.WriteText(5, 10, "System Administration", TN3270Color.Green)
            tn.WriteText(6, 10, "═════════════════════")
            
            tn.WriteText(8, 10, "[U] User Editor")
            tn.WriteText(9, 10, "[S] System Configuration")
            tn.WriteText(10, 10, "[L] View System Log")
            tn.WriteText(11, 10, "[V] Validation Queue")
            tn.WriteText(12, 10, "[B] Bulletin Editor")
            tn.WriteText(13, 10, "[Q] Return to Main Menu")
            
            ' Input Field
            tn.WriteText(16, 10, "Command:")
            tn.AddField(16, 20, 10, "", False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "command")
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, $"Sysop: {session.User.Name.Trim()}  SL:{session.User.SecurityLevel}", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub
        
        Private Sub RenderTelnet(session As ISession)
            session.WriteLine("")
            session.WriteLine("╔══════════════════════════════════════════════════════════════════╗")
            session.WriteLine("║                    WWIV Sysop Menu                               ║")
            session.WriteLine("╚══════════════════════════════════════════════════════════════════╝")
            session.WriteLine("")
            session.WriteLine("  [U] User Editor")
            session.WriteLine("  [S] System Configuration")
            session.WriteLine("  [L] View System Log")
            session.WriteLine("  [V] Validation Queue")
            session.WriteLine("  [B] Bulletin Editor")
            session.WriteLine("  [Q] Return to Main Menu")
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
                        Case "U"
                            ' User Editor
                            session.NavigateTo(New UserEditorScreen())
                            
                        Case "Q"
                            ' Return to main menu
                            session.NavigateTo(New MainMenuScreen())
                            
                        Case "S", "L", "V", "B"
                            ' Show "Not Implemented" message
                            tn.ClearFields()
                            RenderTN3270(session)
                            tn.WriteText(20, 10, $"Command '{cmd}' not yet implemented.", TN3270Color.Yellow)
                            tn.ShowScreen(False)
                            
                        Case Else
                            ' Invalid command
                            tn.ClearFields()
                            RenderTN3270(session)
                            tn.WriteText(20, 10, "Invalid command.", TN3270Color.Red)
                            tn.ShowScreen(False)
                    End Select
                    
                Case &HC3 ' PF3 - Return to main menu
                    session.NavigateTo(New MainMenuScreen())
            End Select
        End Sub
    End Class
End Namespace
