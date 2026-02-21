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
            tn.ClearFields()
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.Red, TN3270Color.Neutral)
            tn.WriteText(1, 28, "WWIV Sysop Menu", TN3270Color.Yellow, TN3270Color.Red)
            
            ' Menu Options
            tn.WriteText(5, 10, "System Administration", TN3270Color.Green)
            tn.WriteText(6, 10, New String("="c, 21))
            
            tn.WriteText(8, 10, "[U] User Editor")
            tn.WriteText(9, 10, "[S] System Configuration")
            tn.WriteText(10, 10, "[L] View System Log")
            tn.WriteText(11, 10, "[V] Validation Queue")
            tn.WriteText(12, 10, "[I] System Information")
            tn.WriteText(13, 10, "[B] Board/Sub Editor")
            tn.WriteText(14, 10, "[Q] Return to Main Menu")
            
            ' Input Field
            tn.WriteText(16, 10, "Command:")
            tn.AddField(16, 20, 10, " ".PadRight(10), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "command")
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, $"Sysop: {session.User.Name.Trim()}  SL:{session.User.SecurityLevel}  PF3=Menu", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub
        
        Private Sub RenderTelnet(session As ISession)
            session.ClearScreen()
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.BgRed) & " WWIV Sysop Menu ".PadRight(70) & Util.Ansi.Reset)
            session.WriteLine("")
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Green) & "  [U] User Editor" & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Green) & "  [S] System Configuration" & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Green) & "  [L] View System Log" & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Green) & "  [V] Validation Queue" & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Green) & "  [I] System Information" & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Green) & "  [B] Board/Sub Editor" & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "  [Q] Return to Main Menu" & Util.Ansi.Reset)
            session.WriteLine("")
            session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "Command: " & Util.Ansi.Reset)
        End Sub
        
        Public Sub HandleInput(session As ISession, input As Object) Implements IScreen.HandleInput
            If TypeOf session Is TN3270SessionAdapter Then
                HandleTN3270Input(DirectCast(session, TN3270SessionAdapter), DirectCast(input, AidKeyEventArgs))
            ElseIf TypeOf input Is String Then
                HandleTelnetInput(session, DirectCast(input, String))
            End If
        End Sub

        Private Sub HandleTelnetInput(session As ISession, input As String)
            Dim cmd = input.Trim().ToUpper()
            Select Case cmd
                Case "U"
                    session.NavigateTo(New UserEditorScreen())
                Case "L"
                    session.NavigateTo(New SystemLogScreen())
                Case "S"
                    session.NavigateTo(New SystemConfigScreen())
                Case "I"
                    session.NavigateTo(New SystemStatusScreen())
                Case "B"
                    session.NavigateTo(New SubBoardEditorScreen())
                Case "Q"
                    session.NavigateTo(New MainMenuScreen())
                Case "V"
                    session.WriteLine("Validation Queue not yet implemented.")
                    RenderTelnet(session)
                Case Else
                    If Not String.IsNullOrEmpty(cmd) Then
                        session.WriteLine("Invalid command.")
                    End If
                    RenderTelnet(session)
            End Select
        End Sub
        
        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            Select Case e.AidKey
                Case AID.ENTER ' ENTER
                    Dim cmd = tn.GetFieldValue("command")?.Trim().ToUpper()
                    
                    Select Case cmd
                        Case "U"
                            session.NavigateTo(New UserEditorScreen())
                            
                        Case "L"
                            session.NavigateTo(New SystemLogScreen())
                            
                        Case "S"
                            session.NavigateTo(New SystemConfigScreen())
                            
                        Case "I"
                            session.NavigateTo(New SystemStatusScreen())
                            
                        Case "B"
                            session.NavigateTo(New SubBoardEditorScreen())
                            
                        Case "Q"
                            ' Return to main menu
                            session.NavigateTo(New MainMenuScreen())
                            
                        Case Else
                            ' Invalid command or handled by session NavigateTo
                            tn.ClearFields()
                            RenderTN3270(session)
                            tn.WriteText(20, 10, "Invalid command.", TN3270Color.Red)
                            tn.ShowScreen(False)
                    End Select
                    
                Case AID.PF3 ' PF3 - Return to main menu
                    session.NavigateTo(New MainMenuScreen())
                Case Else
                    RenderTN3270(session)
            End Select
        End Sub
    End Class
End Namespace
