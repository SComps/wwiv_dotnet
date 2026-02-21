Imports System
Imports System.Text
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports WWIV.Data
Imports WWIV.Services

Namespace WWIV.Screens
    ''' <summary>
    ''' Local Email Screen
    ''' View and send private messages within the BBS
    ''' </summary>
    Public Class LocalEmailScreen
        Implements IScreen
        
        Private _emailService As EmailService
        Private _userService As UserService
        Private _emails As List(Of EmailMessage)
        Private _viewMode As EmailViewMode = EmailViewMode.List
        Private _currentEmailIdx As Integer = -1
        
        Private Enum EmailViewMode
            List
            Read
            Compose
        End Enum
        
        Public Sub New()
            _emailService = New EmailService()
            _userService = New UserService()
        End Sub
        
        Public Sub Activate(session As ISession) Implements IScreen.Activate
            _emails = _emailService.GetEmails(session.User.UserNumber)
            
            If TypeOf session Is TN3270SessionAdapter Then
                RenderTN3270(DirectCast(session, TN3270SessionAdapter))
            Else
                RenderTelnet(session)
            End If
        End Sub
        
        Private Sub RenderTN3270(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            tn.ClearFields() ' Always clear before re-drawing
            
            ' Title Bar
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(1, 30, "WWIV Local Email", TN3270Color.Yellow, TN3270Color.Blue)
            
            Select Case _viewMode
                Case EmailViewMode.List
                    RenderList(tn)
                Case EmailViewMode.Read
                    RenderMessage(tn)
                Case EmailViewMode.Compose
                    RenderCompose(tn)
            End Select
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            Dim statusText = "ENTER=Cmd  S=Send Email  D=Delete  PF3=Menu"
            If _viewMode = EmailViewMode.Read Then statusText = "ENTER=Next  B=Back  D=Delete  R=Reply  PF3=List"
            If _viewMode = EmailViewMode.Compose Then statusText = "ENTER=Send  PF3=Cancel"
            
            tn.WriteText(24, 2, statusText, TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub
        
        Private Sub RenderTelnet(session As ISession)
            session.ClearScreen()
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Cyan, Util.Ansi.BgBlue) & " WWIV Local Email ".PadRight(70) & Util.Ansi.Reset)
            session.WriteLine("")
            
            Select Case _viewMode
                Case EmailViewMode.List
                    RenderTelnetList(session)
                Case EmailViewMode.Read
                    RenderTelnetMessage(session)
                Case EmailViewMode.Compose
                    RenderTelnetCompose(session)
            End Select
        End Sub

        Private Sub RenderTelnetList(session As ISession)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & " #   From                  Date       Subject" & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & New String("-"c, 70) & Util.Ansi.Reset)
            
            If _emails.Count = 0 Then
                session.WriteLine(Util.Ansi.Color(Util.Ansi.Yellow) & "(Empty Mailbox)" & Util.Ansi.Reset)
            Else
                For i = 0 To _emails.Count - 1
                    Dim msg = _emails(i)
                    Dim status = If(msg.IsRead, " ", Util.Ansi.Color(Util.Ansi.Yellow, Util.Ansi.Bold) & "*" & Util.Ansi.Reset)
                    session.WriteLine($"{status}{Util.Ansi.Color(Util.Ansi.White)}{(i + 1).ToString().PadLeft(3)}  " & 
                                     Util.Ansi.Color(Util.Ansi.Green) & msg.FromName.PadRight(20).Substring(0, 20) & "  " & 
                                     Util.Ansi.Color(Util.Ansi.Cyan) & msg.DatePosted.ToString("MM/dd/yy") & "  " & 
                                     Util.Ansi.Color(Util.Ansi.White) & msg.Title & Util.Ansi.Reset)
                Next
            End If
            session.WriteLine("")
            session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "[#] Read, [S] Send, [Q] Quit: " & Util.Ansi.Reset)
        End Sub

        Private Sub RenderTelnetMessage(session As ISession)
            Dim msg = _emails(_currentEmailIdx)
            msg.IsRead = True
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "From    : " & Util.Ansi.Reset & msg.FromName)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "Date    : " & Util.Ansi.Reset & msg.DatePosted)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "Subject : " & Util.Ansi.Reset & Util.Ansi.Color(Util.Ansi.Yellow) & msg.Title & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & New String("-"c, 70) & Util.Ansi.Reset)
            session.WriteLine(msg.Text)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & New String("-"c, 70) & Util.Ansi.Reset)
            session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "(ENTER) Next, (B) Back, (Q) Quit: " & Util.Ansi.Reset)
        End Sub

        Private Sub RenderTelnetCompose(session As ISession)
            session.WriteLine("Compose Private Email")
            session.Write("To (User # or Name): ")
        End Sub

        Private Sub RenderList(tn As TN3270Session)
            tn.WriteText(3, 2, " #   From                  Date       Subject", TN3270Color.Turquoise)
            tn.WriteText(4, 2, New String("-"c, 76))
            
            Dim row = 5
            For i = 0 To Math.Min(_emails.Count - 1, 15)
                Dim msg = _emails(i)
                Dim status = If(msg.IsRead, " ", "*")
                Dim msgNum = (i + 1).ToString().PadLeft(3)
                Dim from = If(msg.FromName?.Trim(), "Unknown").PadRight(20).Substring(0, 20)
                Dim mdate = msg.DatePosted.ToString("MM/dd/yy")
                Dim title = If(msg.Title?.Trim(), "(No Subject)").PadRight(35).Substring(0, 35)
                
                tn.WriteText(row, 2, $"{status}{msgNum}  {from}  {mdate}  {title}")
                row += 1
            Next
            
            If _emails.Count = 0 Then
                tn.WriteText(10, 30, "-- Your mailbox is empty --", TN3270Color.Yellow)
            End If
            
            tn.WriteText(22, 2, "Command:")
            tn.AddField(22, 11, 20, " ".PadRight(20), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "command")
        End Sub
        
        Private Sub RenderMessage(tn As TN3270Session)
            If _currentEmailIdx < 0 OrElse _currentEmailIdx >= _emails.Count Then
                _viewMode = EmailViewMode.List
                RenderList(tn)
                Return
            End If
            
            Dim msg = _emails(_currentEmailIdx)
            msg.IsRead = True
            
            tn.WriteText(3, 2, $"Private Message from {msg.FromName}", TN3270Color.Turquoise)
            tn.WriteText(4, 2, $"Sent on : {msg.DatePosted.ToString()}", TN3270Color.Turquoise)
            tn.WriteText(5, 2, $"Subject : {msg.Title}", TN3270Color.Turquoise)
            tn.WriteText(6, 2, New String("-"c, 76))
            
            Dim bodyLines = (msg.Text & "").Split(New String() {Environment.NewLine, vbLf, vbCr}, StringSplitOptions.None)
            Dim row = 8
            For Each line In bodyLines
                If row > 21 Then Exit For
                tn.WriteText(row, 2, If(line.Length > 76, line.Substring(0, 76), line))
                row += 1
            Next
            
            tn.WriteText(22, 2, "Command:")
            tn.AddField(22, 11, 20, " ".PadRight(20), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "command")
        End Sub
        
        Private Sub RenderCompose(tn As TN3270Session)
            tn.WriteText(4, 10, "TO (User # or Name) :", TN3270Color.Turquoise)
            tn.AddField(4, 32, 20, " ".PadRight(20), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "to_user")
            
            tn.WriteText(6, 10, "Subject            :", TN3270Color.Turquoise)
            tn.AddField(6, 32, 40, " ".PadRight(40), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "subject")
            
            tn.WriteText(8, 10, "Message Text:", TN3270Color.Turquoise)
            tn.AddField(9, 10, 60, " ".PadRight(60), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "line1")
            tn.AddField(10, 10, 60, " ".PadRight(60), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "line2")
            tn.AddField(11, 10, 60, " ".PadRight(60), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "line3")
            tn.AddField(12, 10, 60, " ".PadRight(60), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "line4")
            tn.AddField(13, 10, 60, " ".PadRight(60), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "line5")
        End Sub
        
        Public Sub HandleInput(session As ISession, input As Object) Implements IScreen.HandleInput
            If TypeOf session Is TN3270SessionAdapter Then
                HandleTN3270Input(DirectCast(session, TN3270SessionAdapter), DirectCast(input, AidKeyEventArgs))
            ElseIf TypeOf input Is String Then
                HandleTelnetInput(session, DirectCast(input, String))
            End If
        End Sub

        Private _composeTo As String = ""
        Private _composeSubject As String = ""
        Private _composeBody As New StringBuilder()

        Private Sub HandleTelnetInput(session As ISession, input As String)
            Dim cmd = input.Trim().ToUpper()

            If _viewMode = EmailViewMode.Compose Then
                HandleTelnetComposeInput(session, input)
                Return
            End If

            If _viewMode = EmailViewMode.List Then
                If Integer.TryParse(cmd, Nothing) Then
                    _currentEmailIdx = Integer.Parse(cmd) - 1
                    If _currentEmailIdx >= 0 AndAlso _currentEmailIdx < _emails.Count Then
                        _viewMode = EmailViewMode.Read
                    Else
                        session.WriteLine("Invalid message number.")
                    End If
                Else
                    Select Case cmd
                        Case "Q"
                            session.NavigateTo(New MainMenuScreen())
                            Return
                        Case "S"
                            _viewMode = EmailViewMode.Compose
                            _composeTo = ""
                            _composeSubject = ""
                            _composeBody.Clear()
                        Case Else
                            If Not String.IsNullOrEmpty(cmd) Then session.WriteLine("Invalid command.")
                    End Select
                End If
            ElseIf _viewMode = EmailViewMode.Read Then
                If String.IsNullOrEmpty(cmd) Then
                    If _currentEmailIdx < _emails.Count - 1 Then
                        _currentEmailIdx += 1
                    Else
                        _viewMode = EmailViewMode.List
                    End If
                Else
                    Select Case cmd
                        Case "B", "Q"
                            _viewMode = EmailViewMode.List
                        Case "D"
                            _emailService.DeleteEmail(session.User.UserNumber, _emails(_currentEmailIdx).ID)
                            _emails.RemoveAt(_currentEmailIdx)
                            _viewMode = EmailViewMode.List
                        Case "R"
                            ' Extract useful info for reply
                            Dim original = _emails(_currentEmailIdx)
                            _viewMode = EmailViewMode.Compose
                            _composeTo = original.FromUserNumber.ToString()
                            _composeSubject = "Re: " & original.Title
                            _composeBody.Clear()
                            session.WriteLine("Replying to: " & original.FromName)
                            session.WriteLine("Subject: " & _composeSubject)
                            session.WriteLine("Enter message text (. to finish):")
                        Case Else
                            session.WriteLine("Invalid command.")
                    End Select
                End If
            End If

            RenderTelnet(session)
        End Sub

        Private Sub HandleTelnetComposeInput(session As ISession, input As String)
            If String.IsNullOrEmpty(_composeTo) Then
                _composeTo = input.Trim()
                If String.IsNullOrEmpty(_composeTo) Then
                    session.Write("To (User # or Name): ")
                Else
                    session.Write("Subject: ")
                End If
            ElseIf String.IsNullOrEmpty(_composeSubject) Then
                _composeSubject = input.Trim()
                If String.IsNullOrEmpty(_composeSubject) Then _composeSubject = "(No Subject)"
                session.WriteLine("Enter message text (. on a line by itself to finish):")
            Else
                If input.Trim() = "." Then
                    ' Send the email
                    Dim targetNum = _userService.FindUser(_composeTo)
                    If targetNum > 0 Then
                        Dim targetUser = _userService.GetUser(targetNum)
                        Dim msg As New EmailMessage With {
                            .ToUserNumber = targetNum,
                            .ToName = targetUser.Name,
                            .FromName = session.User.Name,
                            .FromUserNumber = session.User.UserNumber,
                            .Title = _composeSubject,
                            .Text = _composeBody.ToString().Trim(),
                            .DatePosted = DateTime.Now
                        }
                        _emailService.SendEmail(msg)
                        session.WriteLine("Email sent to " & targetUser.Name)
                        _emails = _emailService.GetEmails(session.User.UserNumber)
                        _viewMode = EmailViewMode.List
                        RenderTelnet(session)
                    Else
                        session.WriteLine("User not found!")
                        _viewMode = EmailViewMode.List
                        RenderTelnet(session)
                    End If
                Else
                    _composeBody.AppendLine(input)
                End If
            End If
        End Sub
        
        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            If e.AidKey = AID.ENTER Then ' ENTER
                If _viewMode = EmailViewMode.Compose Then
                    SendCurrent(session)
                    Return
                End If
                
                Dim cmd = tn.GetFieldValue("command")?.Trim().ToUpper()
                
                If _viewMode = EmailViewMode.List Then
                    If Integer.TryParse(cmd, Nothing) Then
                        _currentEmailIdx = Integer.Parse(cmd) - 1
                        _viewMode = EmailViewMode.Read
                    Else
                        Select Case cmd
                            Case "Q"
                                session.NavigateTo(New MainMenuScreen())
                                Return
                            Case "S"
                                _viewMode = EmailViewMode.Compose
                            Case "D"
                                ' Delete selected (Not implemented select yet)
                        End Select
                    End If
                ElseIf _viewMode = EmailViewMode.Read Then
                    Select Case cmd
                        Case "B", "Q"
                            _viewMode = EmailViewMode.List
                        Case "D"
                            _emailService.DeleteEmail(session.User.UserNumber, _emails(_currentEmailIdx).ID)
                            _emails.RemoveAt(_currentEmailIdx)
                            _viewMode = EmailViewMode.List
                        Case "R"
                            ' Reply (Set up compose)
                            _viewMode = EmailViewMode.Compose
                        Case Else
                            ' Default ENTER in Read mode: Next email
                            If _currentEmailIdx < _emails.Count - 1 Then
                                _currentEmailIdx += 1
                            Else
                                _viewMode = EmailViewMode.List
                            End If
                    End Select
                End If
                
                RenderTN3270(session)
            ElseIf e.AidKey = AID.PF3 Then ' PF3
                If _viewMode = EmailViewMode.Compose OrElse _viewMode = EmailViewMode.Read Then
                    _viewMode = EmailViewMode.List
                    RenderTN3270(session)
                Else
                    session.NavigateTo(New MainMenuScreen())
                End If
            Else
                ' On any other key, just re-render to keep terminal active
                RenderTN3270(session)
            End If
        End Sub
        
        Private Sub SendCurrent(session As TN3270SessionAdapter)
            Dim tn = session.TN3270Session
            Dim toUser = tn.GetFieldValue("to_user")?.Trim()
            Dim subject = tn.GetFieldValue("subject")?.Trim()
            Dim body = (tn.GetFieldValue("line1") & vbCrLf & 
                        tn.GetFieldValue("line2") & vbCrLf & 
                        tn.GetFieldValue("line3") & vbCrLf & 
                        tn.GetFieldValue("line4") & vbCrLf & 
                        tn.GetFieldValue("line5")).Trim()
            
            If String.IsNullOrEmpty(toUser) Then
                tn.WriteText(22, 10, "Recipient is required!", TN3270Color.Red)
                tn.ShowScreen(False)
                Return
            End If
            
            Dim targetUserNum = _userService.FindUser(toUser)
            If targetUserNum > 0 Then
                Dim targetUser = _userService.GetUser(targetUserNum)
                Dim msg As New EmailMessage With {
                    .ToUserNumber = targetUserNum,
                    .ToName = targetUser.Name,
                    .FromName = session.User.Name,
                    .FromUserNumber = session.User.UserNumber,
                    .Title = If(String.IsNullOrEmpty(subject), "(No Subject)", subject),
                    .Text = body,
                    .DatePosted = DateTime.Now
                }
                _emailService.SendEmail(msg)
                
                _viewMode = EmailViewMode.List
                _emails = _emailService.GetEmails(session.User.UserNumber) ' Refresh
                RenderTN3270(session)
            Else
                ' User not found
                tn.WriteText(22, 10, $"User '{toUser}' not found!", TN3270Color.Red)
                tn.ShowScreen(False)
            End If
        End Sub
    End Class
End Namespace
