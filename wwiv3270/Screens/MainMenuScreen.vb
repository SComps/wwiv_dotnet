Imports System
Imports System.Linq
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports WWIV.Data

Namespace WWIV.Screens
    Public Class MainMenuScreen
        Implements IScreen

        Private Function GetMainMenu() As MenuDefinition
            Dim configService As New Global.WWIV.Services.ConfigService()
            Dim mainMenu = configService.Menus?.FirstOrDefault(Function(m) m.Id = "Main")
            If mainMenu Is Nothing Then
                ' Provide a safe fallback if menus aren't configured yet
                mainMenu = New MenuDefinition With { .Id = "Main", .Title = "Main Menu" }
                mainMenu.Items.Add(New MenuItem With { .Key = "G", .Description = "Goodbye (Logoff)", .Action = "Logoff" })
            End If
            Return mainMenu
        End Function

        Private Function GetSystemName() As String
            Dim configService As New Global.WWIV.Services.ConfigService()
            Return configService.Config.SystemName
        End Function

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
            Dim bbsName = GetSystemName()
            Dim userName = If(String.IsNullOrEmpty(session.User.Name), "Guest", session.User.Name.Trim())
            
            Dim tnMenu = GetMainMenu()

            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(1, 25, $"{bbsName} - {tnMenu.Title} - {userName}", TN3270Color.Yellow, TN3270Color.Blue)
            
            ' Menu Options
            tn.WriteText(5, 10, bbsName, TN3270Color.Green)
            tn.WriteText(6, 10, tnMenu.Title, TN3270Color.Green)
            tn.WriteText(8, 10, New String("-"c, 40))
            
            Dim row As Integer = 10
            For Each mi In tnMenu.Items.Where(Function(i) session.User.SecurityLevel >= i.MinSecurityLevel)
                ' Determine color based on security level or hardcoded overrides
                Dim color = TN3270Color.Green
                If mi.MinSecurityLevel >= 100 Then color = TN3270Color.Red
                If mi.Key = "?" Then color = TN3270Color.Green
                
                tn.WriteText(row, 10, $"[{mi.Key}] {mi.Description}", color)
                row += 1
            Next
            
            ' Input Field
            row += 2
            tn.WriteText(row, 10, "Command:")
            tn.AddField(row, 20, 10, " ".PadRight(10), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "command")
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, $"SL:{session.User.SecurityLevel}  PF3=Logoff  ENTER=Submit", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub

        Private Sub RenderTelnet(session As ISession)
            Dim bbsName = GetSystemName()
            Dim userName = If(String.IsNullOrEmpty(session.User.Name), "Guest", session.User.Name.Trim())
            Dim telnetMenu = GetMainMenu()

            session.ClearScreen()
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Cyan, Util.Ansi.BgBlue) & $" {bbsName} - {telnetMenu.Title} - {userName} ".PadRight(70) & Util.Ansi.Reset)
            session.WriteLine("")
            
            Dim colors = {Util.Ansi.Green, Util.Ansi.White, Util.Ansi.Yellow, Util.Ansi.Cyan, Util.Ansi.Magenta}
            Dim colorIdx = 0
            
            For Each mi In telnetMenu.Items.Where(Function(i) session.User.SecurityLevel >= i.MinSecurityLevel)
                Dim ansiColor = colors(colorIdx Mod colors.Length)
                If mi.MinSecurityLevel >= 100 Then
                    ansiColor = Util.Ansi.Red & Util.Ansi.Bold
                End If
                session.WriteLine(Util.Ansi.Color(ansiColor) & $"  [{mi.Key}] {mi.Description}" & Util.Ansi.Reset)
                colorIdx += 1
            Next
            
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
        
        Private Sub ProcessAction(session As ISession, actionName As String, rawCmd As String, isTn3270 As Boolean)
            Select Case actionName
                Case "Logoff"
                    If Not isTn3270 Then session.WriteLine("Goodbye!")
                    session.Disconnect()
                Case "Email"
                    session.NavigateTo(New LocalEmailScreen())
                Case "ReadMessages", "PostMessage"
                    session.NavigateTo(New MessageBoardScreen())
                Case "SysopMenu"
                    session.NavigateTo(New SysopMenuScreen())
                Case Else
                    If isTn3270 Then
                        Dim tn = DirectCast(session, TN3270SessionAdapter).TN3270Session
                        tn.ClearFields()
                        RenderTN3270(DirectCast(session, TN3270SessionAdapter))
                        tn.WriteText(22, 10, $"Action '{actionName}' not yet implemented.", TN3270Color.Yellow)
                        tn.ShowScreen(False)
                    Else
                        session.WriteLine($"Action '{actionName}' not yet implemented.")
                        RenderTelnet(session)
                    End If
            End Select
        End Sub

        Private Sub HandleTelnetInput(session As ISession, input As String)
            Dim cmd = input.Trim().ToUpper()
            If String.IsNullOrEmpty(cmd) Then
                RenderTelnet(session)
                Return
            End If
            
            Dim menu = GetMainMenu()
            Dim item = menu.Items.FirstOrDefault(Function(i) i.Key.ToUpper() = cmd)
            
            If item IsNot Nothing AndAlso session.User.SecurityLevel >= item.MinSecurityLevel Then
                ProcessAction(session, item.Action, cmd, False)
            Else
                session.WriteLine("Invalid command.")
                RenderTelnet(session)
            End If
        End Sub

        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            Select Case e.AidKey
                Case AID.ENTER ' ENTER
                    Dim cmd = tn.GetFieldValue("command")?.Trim().ToUpper()
                    Dim menu = GetMainMenu()
                    Dim item = menu.Items.FirstOrDefault(Function(i) i.Key.ToUpper() = cmd)
                    
                    If item IsNot Nothing AndAlso session.User.SecurityLevel >= item.MinSecurityLevel Then
                        ProcessAction(session, item.Action, cmd, True)
                    Else
                        tn.ClearFields()
                        RenderTN3270(session)
                        tn.WriteText(22, 10, "Invalid command. Press ? for help.", TN3270Color.Red)
                        tn.ShowScreen(False)
                    End If
                    
                Case AID.PF3 ' PF3 - Logoff
                    session.Disconnect()
                Case Else
                    RenderTN3270(session)
            End Select
        End Sub
    End Class
End Namespace
