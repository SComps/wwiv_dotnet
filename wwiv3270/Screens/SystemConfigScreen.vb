Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports WWIV.Data
Imports WWIV.Services

Namespace WWIV.Screens
    ''' <summary>
    ''' System Configuration Screen
    ''' Allows Sysop to edit system-wide settings
    ''' </summary>
    Public Class SystemConfigScreen
        Implements IScreen
        
        Private _configService As ConfigService
        Private _tempConfig As SystemConfig
        Private _telnetStep As Integer = 0
        
        Public Sub New()
            _configService = New ConfigService()
            _tempConfig = _configService.Config
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
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.Red, TN3270Color.Neutral)
            tn.WriteText(1, 25, "WWIV System Configuration", TN3270Color.Yellow, TN3270Color.Red)
            
            ' Fields
            tn.WriteText(4, 5, "System Name        :")
            tn.AddField(4, 27, 40, _tempConfig.SystemName.PadRight(40), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "sysname")
            
            tn.WriteText(5, 5, "Sysop Name         :")
            tn.AddField(5, 27, 40, _tempConfig.SysopName.PadRight(40), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "sysopname")
            
            tn.WriteText(7, 5, "New User SL        :")
            tn.AddField(7, 27, 5, _tempConfig.NewUserSecurityLevel.ToString().PadRight(5), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "nusl")
            
            tn.WriteText(8, 5, "New User DSL       :")
            tn.AddField(8, 27, 5, _tempConfig.NewUserDownloadLevel.ToString().PadRight(5), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "nudsl")
            
            tn.WriteText(10, 5, "Closed System      :")
            tn.AddField(10, 27, 3, If(_tempConfig.ClosedSystem, "YES", "NO ").Substring(0, 3), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "closed")
            
            tn.WriteText(11, 5, "Max Users          :")
            tn.AddField(11, 27, 10, _tempConfig.MaxUsers.ToString().PadRight(10), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "maxusers")
            
            tn.WriteText(13, 5, "Data Directory     :")
            tn.AddField(13, 27, 40, _tempConfig.DataDirectory.PadRight(40), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "datadir")
            
            ' Instructions
            tn.WriteText(20, 5, "TAB to move between fields. ENTER to Save. PF3 to Cancel.", TN3270Color.Turquoise)
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, "Configuration Editor", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub

        Private Sub RenderTelnet(session As ISession)
            session.ClearScreen()
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.BgRed) & " WWIV System Configuration ".PadRight(70) & Util.Ansi.Reset)
            session.WriteLine("")
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "1. System Name    : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & _tempConfig.SystemName & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "2. Sysop Name     : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & _tempConfig.SysopName & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "3. New User SL    : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & _tempConfig.NewUserSecurityLevel & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "4. New User DSL   : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & _tempConfig.NewUserDownloadLevel & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "5. Closed System  : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & If(_tempConfig.ClosedSystem, "Yes", "No") & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "6. Max Users      : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & _tempConfig.MaxUsers & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "7. Data Dir       : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & _tempConfig.DataDirectory & Util.Ansi.Reset)
            session.WriteLine("")
            session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "Select item to edit, [S]ave, or [Q]uit: " & Util.Ansi.Reset)
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
            
            If _telnetStep > 0 Then
                HandleTelnetEditStep(session, input)
                Return
            End If

            Select Case cmd
                Case "Q"
                    session.NavigateTo(New SysopMenuScreen())
                    Return
                Case "S"
                    _configService.Config = _tempConfig
                    session.WriteLine("Configuration saved.")
                    session.NavigateTo(New SysopMenuScreen())
                    Return
                Case "1", "2", "3", "4", "5", "6", "7"
                    _telnetStep = Integer.Parse(cmd)
                    Select Case _telnetStep
                        Case 1 : session.Write("Enter System Name: ")
                        Case 2 : session.Write("Enter Sysop Name: ")
                        Case 3 : session.Write("Enter New User SL: ")
                        Case 4 : session.Write("Enter New User DSL: ")
                        Case 5 : session.Write("Closed System? (Y/N): ")
                        Case 6 : session.Write("Enter Max Users: ")
                        Case 7 : session.Write("Enter Data Directory: ")
                    End Select
                    Return
                Case Else
                    If Not String.IsNullOrEmpty(cmd) Then session.WriteLine("Invalid selection.")
            End Select
            
            RenderTelnet(session)
        End Sub

        Private Sub HandleTelnetEditStep(session As ISession, input As String)
            Try
                Select Case _telnetStep
                    Case 1 : _tempConfig.SystemName = input
                    Case 2 : _tempConfig.SysopName = input
                    Case 3 : _tempConfig.NewUserSecurityLevel = Integer.Parse(input)
                    Case 4 : _tempConfig.NewUserDownloadLevel = Integer.Parse(input)
                    Case 5 : _tempConfig.ClosedSystem = input.ToUpper().StartsWith("Y")
                    Case 6 : _tempConfig.MaxUsers = Integer.Parse(input)
                    Case 7 : _tempConfig.DataDirectory = input
                End Select
            Catch
                session.WriteLine("Invalid input. Change ignored.")
            End Try
            
            _telnetStep = 0
            RenderTelnet(session)
        End Sub
        
        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            If e.AidKey = AID.ENTER Then ' ENTER
                SaveConfig(session)
            ElseIf e.AidKey = AID.PF3 Then ' PF3
                session.NavigateTo(New SysopMenuScreen())
            Else
                RenderTN3270(session)
            End If
        End Sub
        
        Private Sub SaveConfig(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            
            Try
                _tempConfig.SystemName = tn.GetFieldValue("sysname")?.Trim()
                _tempConfig.SysopName = tn.GetFieldValue("sysopname")?.Trim()
                
                Dim sl As Integer
                If Integer.TryParse(tn.GetFieldValue("nusl"), sl) Then _tempConfig.NewUserSecurityLevel = sl
                
                Dim dsl As Integer
                If Integer.TryParse(tn.GetFieldValue("nudsl"), dsl) Then _tempConfig.NewUserDownloadLevel = dsl
                
                _tempConfig.ClosedSystem = tn.GetFieldValue("closed")?.Trim().ToUpper().StartsWith("Y")
                
                Dim mu As Integer
                If Integer.TryParse(tn.GetFieldValue("maxusers"), mu) Then _tempConfig.MaxUsers = mu
                
                _tempConfig.DataDirectory = tn.GetFieldValue("datadir")?.Trim()
                
                ' Persist
                _configService.Config = _tempConfig
                
                ' Return
                session.NavigateTo(New SysopMenuScreen())
            Catch ex As Exception
                tn.WriteText(22, 5, $"Error saving config: {ex.Message}", TN3270Color.Red)
                tn.ShowScreen(False)
            End Try
        End Sub
    End Class
End Namespace
