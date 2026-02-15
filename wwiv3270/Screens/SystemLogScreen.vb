Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports wwiv3270.WWIV.Services

Namespace WWIV.Screens
    ''' <summary>
    ''' System Log Viewer Screen
    ''' Displays the system event log
    ''' </summary>
    Public Class SystemLogScreen
        Implements IScreen
        
        Public Sub Activate(session As ISession) Implements IScreen.Activate
            If TypeOf session Is TN3270SessionAdapter Then
                RenderTN3270(DirectCast(session, TN3270SessionAdapter))
            End If
        End Sub
        
        Private Sub RenderTN3270(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            Dim logLines = Logger.GetLogLines(18)
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.Red, TN3270Color.Neutral)
            tn.WriteText(1, 30, "WWIV System Log", TN3270Color.Yellow, TN3270Color.Red)
            
            Dim row = 4
            For Each line In logLines
                tn.WriteText(row, 2, line.PadRight(76).Substring(0, 76), TN3270Color.White)
                row += 1
            Next
            
            If logLines.Count = 0 Then
                tn.WriteText(row, 2, "-- Log is empty --", TN3270Color.Yellow)
            End If
            
            tn.WriteText(22, 10, "Press PF3 or ENTER to Return", TN3270Color.Turquoise)
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, "Event Log Viewer", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub
        
        Public Sub HandleInput(session As ISession, input As Object) Implements IScreen.HandleInput
            session.NavigateTo(New SysopMenuScreen())
        End Sub
    End Class
End Namespace
