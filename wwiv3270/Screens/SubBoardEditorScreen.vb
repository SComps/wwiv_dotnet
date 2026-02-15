Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports wwiv3270.WWIV.Data
Imports wwiv3270.WWIV.Services

Namespace WWIV.Screens
    ''' <summary>
    ''' Sub-board Editor Screen
    ''' List and manage message sub-boards
    ''' </summary>
    Public Class SubBoardEditorScreen
        Implements IScreen
        
        Private _boardService As BoardService
        Private _subBoards As List(Of SubBoard)
        
        Public Sub New()
            _boardService = New BoardService()
        End Sub
        
        Public Sub Activate(session As ISession) Implements IScreen.Activate
            _subBoards = _boardService.GetSubs()
            If TypeOf session Is TN3270SessionAdapter Then
                RenderTN3270(DirectCast(session, TN3270SessionAdapter))
            End If
        End Sub
        
        Private Sub RenderTN3270(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.Red, TN3270Color.Neutral)
            tn.WriteText(1, 28, "WWIV Sub-board Editor", TN3270Color.Yellow, TN3270Color.Red)
            
            tn.WriteText(3, 2, " #   Name                            Read SL  Post SL", TN3270Color.Turquoise)
            tn.WriteText(4, 2, "──────────────────────────────────────────────────────────────────────────")
            
            Dim row = 5
            For i = 0 To Math.Min(_subBoards.Count - 1, 15)
                Dim sb = _subBoards(i)
                Dim num = (i + 1).ToString().PadLeft(3)
                Dim name = sb.Name.PadRight(30).Substring(0, 30)
                Dim rsl = sb.ReadSecurityLevel.ToString().PadLeft(7)
                Dim psl = sb.PostSecurityLevel.ToString().PadLeft(8)
                
                tn.WriteText(row, 2, $"{num}  {name}  {rsl}  {psl}")
                row += 1
            Next
            
            tn.WriteText(22, 2, "Command: [Edit #], [A]dd, [D]elete #, [Q]uit:")
            tn.AddField(22, 48, 20, "", False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "command")
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, $"Total Sub-boards: {_subBoards.Count}", TN3270Color.Yellow, TN3270Color.Blue)
            
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
                Dim cmd = tn.GetFieldValue("command")?.Trim().ToUpper()
                
                If String.IsNullOrEmpty(cmd) OrElse cmd = "Q" Then
                    session.NavigateTo(New SysopMenuScreen())
                ElseIf cmd = "A" Then
                    session.NavigateTo(New SubBoardEditDetailScreen(-1))
                ElseIf cmd.StartsWith("D") Then
                    ' Delete sub logic
                    Dim subNumStr = cmd.Substring(1).Trim()
                    Dim subToDel As Integer
                    If Integer.TryParse(subNumStr, subToDel) Then
                        _boardService.GetSubs().RemoveAt(subToDel - 1)
                        _boardService.SaveSubs()
                    End If
                ElseIf Integer.TryParse(cmd, Nothing) Then
                    session.NavigateTo(New SubBoardEditDetailScreen(Integer.Parse(cmd) - 1))
                End If
                
                RenderTN3270(session)
            ElseIf e.AidKey = &HC3 Then ' PF3
                session.NavigateTo(New SysopMenuScreen())
            End If
        End Sub
    End Class
End Namespace
