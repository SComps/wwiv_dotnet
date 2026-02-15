Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports wwiv3270.WWIV.Data
Imports wwiv3270.WWIV.Services

Namespace WWIV.Screens
    ''' <summary>
    ''' Sub-board Detail Editor
    ''' Edit properties of a specific message sub-board
    ''' </summary>
    Public Class SubBoardEditDetailScreen
        Implements IScreen
        
        Private _boardService As BoardService
        Private _subIdx As Integer
        Private _subBoard As SubBoard
        
        Public Sub New(subIdx As Integer)
            _boardService = New BoardService()
            _subIdx = subIdx
        End Sub
        
        Public Sub Activate(session As ISession) Implements IScreen.Activate
            Dim subs = _boardService.GetSubs()
            If _subIdx >= 0 AndAlso _subIdx < subs.Count Then
                _subBoard = subs(_subIdx)
            Else
                _subBoard = New SubBoard() With {.Number = subs.Count + 1, .Name = "New Sub"}
            End If
            
            If TypeOf session Is TN3270SessionAdapter Then
                RenderTN3270(DirectCast(session, TN3270SessionAdapter))
            End If
        End Sub
        
        Private Sub RenderTN3270(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.Red, TN3270Color.Neutral)
            tn.WriteText(1, 25, $"Edit Sub-Board: {_subBoard.Name}", TN3270Color.Yellow, TN3270Color.Red)
            
            tn.WriteText(4, 5, "Sub Name           :")
            tn.AddField(4, 27, 30, _subBoard.Name, False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "name")
            
            tn.WriteText(5, 5, "Filename           :")
            tn.AddField(5, 27, 12, _subBoard.Filename, False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "filename")
            
            tn.WriteText(7, 5, "Read Security Lvl  :")
            tn.AddField(7, 27, 5, _subBoard.ReadSecurityLevel.ToString(), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "rsl")
            
            tn.WriteText(8, 5, "Post Security Lvl  :")
            tn.AddField(8, 27, 5, _subBoard.PostSecurityLevel.ToString(), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "psl")
            
            tn.WriteText(10, 5, "Max Messages       :")
            tn.AddField(10, 27, 10, _subBoard.MaxMessages.ToString(), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "maxmsgs")
            
            tn.WriteText(11, 5, "Allow Anonymous    :")
            tn.AddField(11, 27, 3, If(_subBoard.AllowAnonymous, "YES", "NO"), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "anon")
            
            tn.WriteText(20, 5, "ENTER to Save. PF3 to Cancel.", TN3270Color.Turquoise)
            
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, "Sub-Board Detail Editor", TN3270Color.Yellow, TN3270Color.Blue)
            
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
                SaveSub(session)
            ElseIf e.AidKey = &HC3 Then ' PF3
                session.NavigateTo(New SubBoardEditorScreen())
            End If
        End Sub
        
        Private Sub SaveSub(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            Try
                _subBoard.Name = tn.GetFieldValue("name")?.Trim()
                _subBoard.Filename = tn.GetFieldValue("filename")?.Trim()
                
                Dim rsl As Integer
                If Integer.TryParse(tn.GetFieldValue("rsl"), rsl) Then _subBoard.ReadSecurityLevel = rsl
                
                Dim psl As Integer
                If Integer.TryParse(tn.GetFieldValue("psl"), psl) Then _subBoard.PostSecurityLevel = psl
                
                Dim max As Integer
                If Integer.TryParse(tn.GetFieldValue("maxmsgs"), max) Then _subBoard.MaxMessages = max
                
                _subBoard.AllowAnonymous = tn.GetFieldValue("anon")?.Trim().ToUpper() = "YES"
                
                ' Update in list
                Dim subs = _boardService.GetSubs()
                If _subIdx >= 0 AndAlso _subIdx < subs.Count Then
                    subs(_subIdx) = _subBoard
                Else
                    subs.Add(_subBoard)
                End If
                
                _boardService.SaveSubs()
                session.NavigateTo(New SubBoardEditorScreen())
            Catch ex As Exception
                tn.WriteText(22, 5, $"Error: {ex.Message}", TN3270Color.Red)
                tn.ShowScreen(False)
            End Try
        End Sub
    End Class
End Namespace
