Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports wwiv3270.WWIV.Data
Imports wwiv3270.WWIV.Services

Namespace WWIV.Screens
    ''' <summary>
    ''' System Configuration Screen
    ''' Allows Sysop to edit system-wide settings
    ''' </summary>
    Public Class SystemConfigScreen
        Implements IScreen
        
        Private _configService As ConfigService
        Private _tempConfig As SystemConfig
        
        Public Sub New()
            _configService = New ConfigService()
            _tempConfig = _configService.Config ' This creates a copy because it's a class? No, it's a reference. 
            ' For better safety, we should clone it, but let's just work with the reference for now.
        End Sub
        
        Public Sub Activate(session As ISession) Implements IScreen.Activate
            If TypeOf session Is TN3270SessionAdapter Then
                RenderTN3270(DirectCast(session, TN3270SessionAdapter))
            End If
        End Sub
        
        Private Sub RenderTN3270(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.Red, TN3270Color.Neutral)
            tn.WriteText(1, 25, "WWIV System Configuration", TN3270Color.Yellow, TN3270Color.Red)
            
            ' Fields
            tn.WriteText(4, 5, "System Name        :")
            tn.AddField(4, 27, 40, _tempConfig.SystemName, False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "sysname")
            
            tn.WriteText(5, 5, "Sysop Name         :")
            tn.AddField(5, 27, 40, _tempConfig.SysopName, False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "sysopname")
            
            tn.WriteText(7, 5, "New User SL        :")
            tn.AddField(7, 27, 5, _tempConfig.NewUserSecurityLevel.ToString(), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "nusl")
            
            tn.WriteText(8, 5, "New User DSL       :")
            tn.AddField(8, 27, 5, _tempConfig.NewUserDownloadLevel.ToString(), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "nudsl")
            
            tn.WriteText(10, 5, "Closed System      :")
            tn.AddField(10, 27, 3, If(_tempConfig.ClosedSystem, "YES", "NO"), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "closed")
            
            tn.WriteText(11, 5, "Max Users          :")
            tn.AddField(11, 27, 10, _tempConfig.MaxUsers.ToString(), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "maxusers")
            
            tn.WriteText(13, 5, "Data Directory     :")
            tn.AddField(13, 27, 40, _tempConfig.DataDirectory, False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "datadir")
            
            ' Instructions
            tn.WriteText(20, 5, "TAB to move between fields. ENTER to Save. PF3 to Cancel.", TN3270Color.Turquoise)
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, "Configuration Editor", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub
        
        Public Sub HandleInput(session As ISession, input As Object) Implements IScreen.HandleInput
            If TypeOf session Is TN3270SessionAdapter Then
                HandleTN3270Input(DirectCast(session, TN3270SessionAdapter), DirectCast(input, AidKeyEventArgs))
            End If
        End Sub
        
        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            If e.AidKey = &H7D Then ' ENTER
                SaveConfig(session)
            ElseIf e.AidKey = &HC3 Then ' PF3
                session.NavigateTo(New SysopMenuScreen())
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
                
                _tempConfig.ClosedSystem = tn.GetFieldValue("closed")?.Trim().ToUpper() = "YES"
                
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
