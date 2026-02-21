Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports WWIV.Data
Imports WWIV.Services

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
        Private _telnetStep As Integer = 0
        
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
            Else
                RenderTelnet(session)
            End If
        End Sub
        
        Private Sub RenderTN3270(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            tn.ClearFields()
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.Red, TN3270Color.Neutral)
            tn.WriteText(1, 25, $"Edit Sub-Board: {_subBoard.Name}", TN3270Color.Yellow, TN3270Color.Red)
            
            tn.WriteText(4, 5, "Sub Name           :")
            tn.AddField(4, 27, 30, _subBoard.Name.PadRight(30), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "name")
            
            tn.WriteText(5, 5, "Filename           :")
            tn.AddField(5, 27, 12, _subBoard.Filename.PadRight(12), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "filename")
            
            tn.WriteText(7, 5, "Read Security Lvl  :")
            tn.AddField(7, 27, 5, _subBoard.ReadSecurityLevel.ToString().PadRight(5), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "rsl")
            
            tn.WriteText(8, 5, "Post Security Lvl  :")
            tn.AddField(8, 27, 5, _subBoard.PostSecurityLevel.ToString().PadRight(5), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "psl")
            
            tn.WriteText(10, 5, "Max Messages       :")
            tn.AddField(10, 27, 10, _subBoard.MaxMessages.ToString().PadRight(10), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "maxmsgs")
            
            tn.WriteText(11, 5, "Allow Anonymous    :")
            tn.AddField(11, 27, 3, If(_subBoard.AllowAnonymous, "YES", "NO ").Substring(0, 3), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "anon")
            
            tn.WriteText(20, 5, "ENTER to Save. PF3 to Cancel.", TN3270Color.Turquoise)
            
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, "Sub-Board Detail Editor", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub

        Private Sub RenderTelnet(session As ISession)
            session.ClearScreen()
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.BgRed) & $" Edit Sub-Board: {_subBoard.Name} ".PadRight(70) & Util.Ansi.Reset)
            session.WriteLine("")
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "1. Name        : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & _subBoard.Name & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "2. Filename    : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & _subBoard.Filename & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "3. Read SL     : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & _subBoard.ReadSecurityLevel & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "4. Post SL     : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & _subBoard.PostSecurityLevel & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "5. Max Msgs    : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & _subBoard.MaxMessages & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "6. Anonymous   : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Green) & If(_subBoard.AllowAnonymous, "Yes", "No") & Util.Ansi.Reset)
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
                    session.NavigateTo(New SubBoardEditorScreen())
                    Return
                Case "S"
                    SaveSubData()
                    session.WriteLine("Sub-board saved.")
                    session.NavigateTo(New SubBoardEditorScreen())
                    Return
                Case "1", "2", "3", "4", "5", "6"
                    _telnetStep = Integer.Parse(cmd)
                    Select Case _telnetStep
                        Case 1 : session.Write("Enter Sub Name: ")
                        Case 2 : session.Write("Enter Filename: ")
                        Case 3 : session.Write("Enter Read SL: ")
                        Case 4 : session.Write("Enter Post SL: ")
                        Case 5 : session.Write("Enter Max Messages: ")
                        Case 6 : session.Write("Allow Anonymous? (Y/N): ")
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
                    Case 1 : _subBoard.Name = input
                    Case 2 : _subBoard.Filename = input
                    Case 3 : _subBoard.ReadSecurityLevel = Integer.Parse(input)
                    Case 4 : _subBoard.PostSecurityLevel = Integer.Parse(input)
                    Case 5 : _subBoard.MaxMessages = Integer.Parse(input)
                    Case 6 : _subBoard.AllowAnonymous = input.ToUpper().StartsWith("Y")
                End Select
            Catch
                session.WriteLine("Invalid input.")
            End Try
            
            _telnetStep = 0
            RenderTelnet(session)
        End Sub
        
        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            If e.AidKey = AID.ENTER Then ' ENTER
                Try
                    _subBoard.Name = tn.GetFieldValue("name")?.Trim()
                    _subBoard.Filename = tn.GetFieldValue("filename")?.Trim()
                    
                    Dim val As Integer
                    If Integer.TryParse(tn.GetFieldValue("rsl"), val) Then _subBoard.ReadSecurityLevel = val
                    If Integer.TryParse(tn.GetFieldValue("psl"), val) Then _subBoard.PostSecurityLevel = val
                    If Integer.TryParse(tn.GetFieldValue("maxmsgs"), val) Then _subBoard.MaxMessages = val
                    
                    _subBoard.AllowAnonymous = tn.GetFieldValue("anon")?.Trim().ToUpper().StartsWith("Y")
                    
                    SaveSubData()
                    session.NavigateTo(New SubBoardEditorScreen())
                Catch ex As Exception
                    tn.WriteText(22, 5, $"Error: {ex.Message}", TN3270Color.Red)
                    tn.ShowScreen(False)
                End Try
            ElseIf e.AidKey = AID.PF3 Then ' PF3
                session.NavigateTo(New SubBoardEditorScreen())
            Else
                RenderTN3270(session)
            End If
        End Sub

        Private Sub SaveSubData()
            Dim subs = _boardService.GetSubs()
            if _subIdx >= 0 AndAlso _subIdx < subs.Count Then
                subs(_subIdx) = _subBoard
            Else
                subs.Add(_subBoard)
            End If
            _boardService.SaveSubs()
        End Sub
    End Class
End Namespace
