Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports wwiv3270.WWIV.Data
Imports wwiv3270.WWIV.Services

Namespace WWIV.Screens
    ''' <summary>
    ''' Message Board Screen
    ''' Display message headers and allow reading
    ''' </summary>
    Public Class MessageBoardScreen
        Implements IScreen
        
        Private _boardService As BoardService
        Private _subBoards As List(Of SubBoard)
        Private _currentSubIdx As Integer = 0
        Private _messages As List(Of MessageHeader)
        Private _viewMode As MessageViewMode = MessageViewMode.List
        Private _currentMessageIdx As Integer = -1
        
        Private Enum MessageViewMode
            List
            Read
            Join
        End Enum
        
        Public Sub New(Optional subIdx As Integer = 0)
            _boardService = New BoardService()
            _currentSubIdx = subIdx
        End Sub
        
        Public Sub Activate(session As ISession) Implements IScreen.Activate
            _subBoards = _boardService.GetSubs()
            If _currentSubIdx >= _subBoards.Count Then _currentSubIdx = 0
            
            _messages = _boardService.GetMessages(_subBoards(_currentSubIdx).Number)
            
            If TypeOf session Is TN3270SessionAdapter Then
                RenderTN3270(DirectCast(session, TN3270SessionAdapter))
            End If
        End Sub
        
        Private Sub RenderTN3270(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            Dim subBoard = _subBoards(_currentSubIdx)
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(1, 20, $"WWIV Message Board: {subBoard.Name}", TN3270Color.Yellow, TN3270Color.Blue)
            
            If _viewMode = MessageViewMode.List Then
                RenderList(tn)
            ElseIf _viewMode = MessageViewMode.Read Then
                RenderMessage(tn)
            Else
                RenderJoin(tn)
            End If
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            Dim statusText = If(_viewMode = MessageViewMode.List, "ENTER=Read  R=Read #  P=Post  J=Join  Q=Main Menu", "ENTER=Next  B=Back to List  A=Answer  Q=Quit Read")
            tn.WriteText(24, 2, statusText, TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub
        
        Private Sub RenderList(tn As TN3270Session)
            tn.WriteText(3, 2, " #   From                  Date       Subject", TN3270Color.Turquoise)
            tn.WriteText(4, 2, "──────────────────────────────────────────────────────────────────────────")
            
            Dim row = 5
            Dim startIdx = Math.Max(0, _messages.Count - 15) ' Show last 15
            
            For i = startIdx To _messages.Count - 1
                Dim msg = _messages(i)
                Dim msgNum = (i + 1).ToString().PadLeft(3)
                Dim from = msg.FromName.Trim().PadRight(20).Substring(0, 20)
                Dim mdate = msg.DatePosted.ToString("MM/dd/yy")
                Dim title = msg.Title.Trim().PadRight(35).Substring(0, 35)
                
                tn.WriteText(row, 2, $"{msgNum}  {from}  {mdate}  {title}")
                row += 1
            Next
            
            tn.WriteText(22, 2, "Command:")
            tn.AddField(22, 11, 20, "", False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "command")
        End Sub
        
        Private Sub RenderMessage(tn As TN3270Session)
            If _currentMessageIdx < 0 OrElse _currentMessageIdx >= _messages.Count Then
                _viewMode = MessageViewMode.List
                RenderList(tn)
                Return
            End If
            
            Dim msg = _messages(_currentMessageIdx)
            
            tn.WriteText(3, 2, $"Msg #   : {_currentMessageIdx + 1} of {_messages.Count}", TN3270Color.Turquoise)
            tn.WriteText(4, 2, $"From    : {msg.FromName}", TN3270Color.Turquoise)
            tn.WriteText(5, 2, $"Date    : {msg.DatePosted.ToString()}", TN3270Color.Turquoise)
            tn.WriteText(6, 2, $"Subject : {msg.Title}", TN3270Color.Turquoise)
            tn.WriteText(7, 2, "──────────────────────────────────────────────────────────────────────────")
            
            ' Message Body
            Dim bodyLines = msg.Text.Split(New String() {Environment.NewLine}, StringSplitOptions.None)
            Dim row = 8
            For Each line In bodyLines
                If row > 21 Then Exit For
                tn.WriteText(row, 2, line)
                row += 1
            Next
            
            tn.WriteText(22, 2, "Command (ENTER for next):")
            tn.AddField(22, 28, 20, "", False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "command")
        End Sub
        
        Private Sub RenderJoin(tn As TN3270Session)
            tn.WriteText(3, 2, "Select a Sub-Board to Join:", TN3270Color.Turquoise)
            tn.WriteText(4, 2, "──────────────────────────────────────────────────────────────────────────")
            
            Dim row = 5
            For i = 0 To _subBoards.Count - 1
                Dim subBoard = _subBoards(i)
                Dim selector = If(i = _currentSubIdx, ">>", "  ")
                tn.WriteText(row, 2, $"{selector} {i + 1}. {subBoard.Name}")
                row += 1
            Next
            
            tn.WriteText(22, 2, "Enter Sub # to Join:")
            tn.AddField(22, 24, 10, "", False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "command")
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
                
                If _viewMode = MessageViewMode.List Then
                    If String.IsNullOrEmpty(cmd) Then
                        _viewMode = MessageViewMode.List
                    ElseIf Integer.TryParse(cmd, Nothing) Then
                        _currentMessageIdx = Integer.Parse(cmd) - 1
                        _viewMode = MessageViewMode.Read
                    Else
                        Select Case cmd
                            Case "Q"
                                session.NavigateTo(New MainMenuScreen())
                                Return
                            Case "J"
                                _viewMode = MessageViewMode.Join
                            Case "P"
                                ' Post (Not implemented)
                        End Select
                    End If
                ElseIf _viewMode = MessageViewMode.Join Then
                    If Integer.TryParse(cmd, Nothing) Then
                        Dim newSub = Integer.Parse(cmd) - 1
                        If newSub >= 0 AndAlso newSub < _subBoards.Count Then
                            _currentSubIdx = newSub
                            _messages = _boardService.GetMessages(_subBoards(_currentSubIdx).Number)
                            _viewMode = MessageViewMode.List
                        End If
                    ElseIf cmd = "Q" Then
                        _viewMode = MessageViewMode.List
                    End If
                Else
                    ' Read Mode
                    If String.IsNullOrEmpty(cmd) Then
                        ' Next message
                        If _currentMessageIdx < _messages.Count - 1 Then
                            _currentMessageIdx += 1
                        Else
                            _viewMode = MessageViewMode.List
                        End If
                    Else
                        Select Case cmd
                            Case "Q", "B"
                                _viewMode = MessageViewMode.List
                        End Select
                    End If
                End If
                
                RenderTN3270(session)
            ElseIf e.AidKey = &HC3 Then ' PF3
                session.NavigateTo(New MainMenuScreen())
            End If
        End Sub
    End Class
End Namespace
