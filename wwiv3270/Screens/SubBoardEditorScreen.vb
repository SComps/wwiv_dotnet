Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports WWIV.Data
Imports WWIV.Services

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
            Else
                RenderTelnet(session)
            End If
        End Sub
        
        Private Sub RenderTN3270(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            tn.ClearFields()
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.Red, TN3270Color.Neutral)
            tn.WriteText(1, 28, "WWIV Sub-board Editor", TN3270Color.Yellow, TN3270Color.Red)
            
            tn.WriteText(3, 2, " #   Name                            Read SL  Post SL", TN3270Color.Turquoise)
            tn.WriteText(4, 2, New String("-"c, 76))
            
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
            tn.AddField(22, 48, 20, " ".PadRight(20), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "command")
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, $"Total Sub-boards: {_subBoards.Count}", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub

        Private Sub RenderTelnet(session As ISession)
            session.ClearScreen()
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.BgRed) & " WWIV Sub-board Editor ".PadRight(70) & Util.Ansi.Reset)
            session.WriteLine("")
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & " #   Name                            Read SL  Post SL" & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & New String("-"c, 70) & Util.Ansi.Reset)
            For i = 0 To _subBoards.Count - 1
                Dim sb = _subBoards(i)
                session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & (i + 1).ToString().PadLeft(3) & "  " & 
                                 Util.Ansi.Color(Util.Ansi.Green) & sb.Name.PadRight(30).Substring(0, 30) & "  " & 
                                 Util.Ansi.Color(Util.Ansi.Cyan) & sb.ReadSecurityLevel.ToString().PadLeft(7) & "  " & 
                                 Util.Ansi.Color(Util.Ansi.Cyan) & sb.PostSecurityLevel.ToString().PadLeft(7) & Util.Ansi.Reset)
            Next
            session.WriteLine("")
            session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "Command ([#] Edit, [A]dd, [D#] Delete, [Q]uit): " & Util.Ansi.Reset)
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
            
            If String.IsNullOrEmpty(cmd) OrElse cmd = "Q" Then
                session.NavigateTo(New SysopMenuScreen())
            ElseIf cmd = "A" Then
                session.NavigateTo(New SubBoardEditDetailScreen(-1))
            ElseIf cmd.StartsWith("D") Then
                Dim subNumStr = cmd.Substring(1).Trim()
                Dim subToDel As Integer
                If Integer.TryParse(subNumStr, subToDel) AndAlso subToDel > 0 AndAlso subToDel <= _subBoards.Count Then
                    _subBoards.RemoveAt(subToDel - 1)
                    _boardService.SaveSubs()
                    session.WriteLine("Sub-board deleted.")
                    RenderTelnet(session)
                Else
                    session.WriteLine("Invalid sub-board number.")
                    RenderTelnet(session)
                End If
            ElseIf Integer.TryParse(cmd, Nothing) Then
                Dim subToEdit = Integer.Parse(cmd) - 1
                If subToEdit >= 0 AndAlso subToEdit < _subBoards.Count Then
                    session.NavigateTo(New SubBoardEditDetailScreen(subToEdit))
                Else
                    session.WriteLine("Invalid sub-board number.")
                    RenderTelnet(session)
                End If
            Else
                session.WriteLine("Invalid command.")
                RenderTelnet(session)
            End If
        End Sub
        
        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            If e.AidKey = AID.ENTER Then ' ENTER
                Dim cmd = tn.GetFieldValue("command")?.Trim().ToUpper()
                
                If String.IsNullOrEmpty(cmd) OrElse cmd = "Q" Then
                    session.NavigateTo(New SysopMenuScreen())
                ElseIf cmd = "A" Then
                    session.NavigateTo(New SubBoardEditDetailScreen(-1))
                ElseIf cmd.StartsWith("D") Then
                    ' Delete sub logic
                    Dim subNumStr = cmd.Substring(1).Trim()
                    Dim subToDel As Integer
                    If Integer.TryParse(subNumStr, subToDel) AndAlso subToDel > 0 AndAlso subToDel <= _subBoards.Count Then
                        _subBoards.RemoveAt(subToDel - 1)
                        _boardService.SaveSubs()
                    End If
                    RenderTN3270(session)
                ElseIf Integer.TryParse(cmd, Nothing) Then
                    Dim subToEdit = Integer.Parse(cmd) - 1
                    If subToEdit >= 0 AndAlso subToEdit < _subBoards.Count Then
                        session.NavigateTo(New SubBoardEditDetailScreen(subToEdit))
                    Else
                        RenderTN3270(session)
                    End If
                Else
                    RenderTN3270(session)
                End If
            ElseIf e.AidKey = AID.PF3 Then ' PF3
                session.NavigateTo(New SysopMenuScreen())
            Else
                RenderTN3270(session)
            End If
        End Sub
    End Class
End Namespace
