Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports WWIV.Data
Imports WWIV.Services

Namespace WWIV.Screens
    ''' <summary>
    ''' Sysop User Editor Screen
    ''' Allows system operator to view and edit user accounts
    ''' </summary>
    Public Class UserEditorScreen
        Implements IScreen
        
        Private _userService As UserService
        Private _userList As List(Of UserRecord)
        Private _selectedIndex As Integer = 0
        Private _editMode As Boolean = False
        Private _editField As String = ""
        
        Public Sub New()
            _userService = New UserService()
        End Sub
        
        Public Sub Activate(session As ISession) Implements IScreen.Activate
            ' Load all users
            _userList = _userService.GetAllUsers()
            
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
            tn.AddField(1, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(1, 30, "WWIV User Editor", TN3270Color.Yellow, TN3270Color.Blue)
            
            ' User List Header
            tn.WriteText(3, 2, "Num", TN3270Color.Yellow)
            tn.WriteText(3, 7, "Name", TN3270Color.Yellow)
            tn.WriteText(3, 38, "Real Name", TN3270Color.Yellow)
            tn.WriteText(3, 60, "SL", TN3270Color.Yellow)
            tn.WriteText(3, 64, "Logons", TN3270Color.Yellow)
            tn.WriteText(3, 72, "Last On", TN3270Color.Yellow)
            
            ' Display users (up to 15 on screen)
            Dim startIdx = Math.Max(0, _selectedIndex - 7)
            Dim endIdx = Math.Min(_userList.Count - 1, startIdx + 14)
            
            Dim row = 5
            If _userList.Count = 0 Then
                tn.WriteText(row, 2, "(No users found)")
            Else
                For i = startIdx To endIdx
                    Dim user = _userList(i)
                    
                    Dim color = If(i = _selectedIndex, TN3270Color.Green, TN3270Color.Neutral)
                    Dim highlight = If(i = _selectedIndex, TN3270Highlight.ReverseVideo, TN3270Highlight.None)
                    
                    tn.AddField(row, 2, 4, user.UserNumber.ToString().PadLeft(4), True, color, TN3270Color.Neutral, highlight)
                    tn.AddField(row, 7, 30, user.Name.Trim().PadRight(30).Substring(0, Math.Min(30, user.Name.Trim().Length)).PadRight(30), True, color, TN3270Color.Neutral, highlight)
                    tn.AddField(row, 38, 20, user.RealName.Trim().PadRight(20).Substring(0, Math.Min(20, user.RealName.Trim().Length)).PadRight(20), True, color, TN3270Color.Neutral, highlight)
                    tn.AddField(row, 60, 3, user.SecurityLevel.ToString().PadLeft(3), True, color, TN3270Color.Neutral, highlight)
                    tn.AddField(row, 64, 6, user.TotalLogons.ToString().PadLeft(6), True, color, TN3270Color.Neutral, highlight)
                    tn.AddField(row, 72, 8, user.LastLogon.ToString("MM/dd/yy"), True, color, TN3270Color.Neutral, highlight)
                    
                    row += 1
                Next
            End If
            
            ' Status Row / Command Row
            tn.WriteText(21, 1, New String("-"c, 80), TN3270Color.Blue)
            
            ' Edit Panel (if in edit mode)
            If _editMode AndAlso _selectedIndex >= 0 AndAlso _selectedIndex < _userList.Count Then
                Dim user = _userList(_selectedIndex)
                
                tn.WriteText(22, 2, $"Editing User #{user.UserNumber}: {user.Name.Trim()}", TN3270Color.Green)
                Select Case _editField
                    Case "sl"
                        tn.WriteText(22, 40, "Security Level (0-255):")
                        tn.AddField(22, 63, 3, user.SecurityLevel.ToString().PadRight(3), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "editvalue")
                    Case "dsl"
                        tn.WriteText(22, 40, "DL Security (0-255)  :")
                        tn.AddField(22, 63, 3, user.DownloadSecurityLevel.ToString().PadRight(3), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "editvalue")
                    Case "note"
                        tn.WriteText(22, 35, "Note:")
                        tn.AddField(22, 41, 35, user.Note.Trim().PadRight(35), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "editvalue")
                    Case "delete"
                        If user.UserNumber = 1 Then
                            tn.WriteText(22, 40, "CANNOT DELETE USER #1!", TN3270Color.Red)
                        Else
                            tn.WriteText(22, 40, "Delete user? (Y/N):")
                            tn.AddField(22, 61, 1, " ", False, TN3270Color.Red, TN3270Color.Neutral, TN3270Highlight.Underline, "editvalue")
                        End If
                End Select
            Else
                ' Command Line when not in edit mode
                tn.WriteText(22, 2, "Command / Jump to User # / Search Name:", TN3270Color.Yellow)
                tn.AddField(22, 42, 30, " ".PadRight(30), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "command")
            End If
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            Dim statusText = If(_editMode, "ENTER=Save  PF3=Cancel", "PF1=Up  PF2=Down  ENTER=Submit  E,D,N,X=Edit  PF3=Quit")
            tn.WriteText(24, 2, statusText, TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub
        
        Private Sub RenderTelnet(session As ISession)
            session.ClearScreen()
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Cyan, Util.Ansi.BgBlue) & " WWIV User Editor ".PadRight(80) & Util.Ansi.Reset)
            session.WriteLine("")
            session.WriteLine(Util.Ansi.Color(Util.Ansi.Turquoise) & "Num   Name                            Real Name             SL  Logons  Last On" & Util.Ansi.Reset)
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "----  ------------------------------  --------------------  --  ------  --------" & Util.Ansi.Reset)
            
            ' Show around 10-15 users centered on _selectedIndex
            Dim startIdx = Math.Max(0, _selectedIndex - 5)
            Dim endIdx = Math.Min(_userList.Count - 1, startIdx + 11)
            
            For i = startIdx To endIdx
                Dim user = _userList(i)
                Dim isSelected = (i = _selectedIndex)
                Dim line = $"{user.UserNumber,3}  {user.Name.Trim().PadRight(30).Substring(0, 30)}  {user.RealName.Trim().PadRight(20).Substring(0, 20)}  {user.SecurityLevel,2}  {user.TotalLogons,6}  {user.LastLogon.ToString("MM/dd/yy")}"
                
                If isSelected Then
                    session.WriteLine(Util.Ansi.Reverse & ">" & line & Util.Ansi.Reset)
                Else
                    session.WriteLine(" " & line)
                End If
            Next
            
            session.WriteLine("")
            If _editMode Then
                Dim user = _userList(_selectedIndex)
                Select Case _editField
                    Case "sl" : session.Write(Util.Ansi.Color(Util.Ansi.Yellow) & $"Security Level (0-255) [{user.SecurityLevel}]: " & Util.Ansi.Reset)
                    Case "dsl" : session.Write(Util.Ansi.Color(Util.Ansi.Yellow) & $"DL Security (0-255) [{user.DownloadSecurityLevel}]: " & Util.Ansi.Reset)
                    Case "note" : session.Write(Util.Ansi.Color(Util.Ansi.Yellow) & $"Note [{user.Note.Trim()}]: " & Util.Ansi.Reset)
                    Case "delete" : session.Write(Util.Ansi.Color(Util.Ansi.Red, Util.Ansi.Bold) & $"Delete user #{user.UserNumber}? (Y/N): " & Util.Ansi.Reset)
                End Select
            Else
                session.WriteLine(Util.Ansi.Color(Util.Ansi.White) & "Commands: [N]ext, [B]ack, [E]dsl, [D]sl, [T]note, [X]delete, [Q]uit" & Util.Ansi.Reset)
                session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "Command or Search: " & Util.Ansi.Reset)
            End If
        End Sub
        
        Public Sub HandleInput(session As ISession, input As Object) Implements IScreen.HandleInput
            If TypeOf session Is TN3270SessionAdapter Then
                HandleTN3270Input(DirectCast(session, TN3270SessionAdapter), DirectCast(input, AidKeyEventArgs))
            ElseIf TypeOf input Is String Then
                HandleTelnetInput(session, DirectCast(input, String))
            End If
        End Sub

        Private Sub HandleTelnetInput(session As ISession, input As String)
            Dim cmd = input.Trim()
            
            If _editMode Then
                HandleTelnetEditInput(session, cmd)
                Return
            End If

            Select Case cmd.ToUpper()
                Case "Q"
                    session.NavigateTo(New SysopMenuScreen())
                    Return
                Case "N"
                    If _selectedIndex < _userList.Count - 1 Then _selectedIndex += 1
                Case "B"
                    If _selectedIndex > 0 Then _selectedIndex -= 1
                Case "E"
                    _editMode = True : _editField = "sl"
                Case "D"
                    _editMode = True : _editField = "dsl"
                Case "T"
                    _editMode = True : _editField = "note"
                Case "X"
                    _editMode = True : _editField = "delete"
                Case Else
                    If Not String.IsNullOrEmpty(cmd) Then
                        ProcessCommand(cmd)
                    End If
            End Select
            
            RenderTelnet(session)
        End Sub

        Private Sub HandleTelnetEditInput(session As ISession, input As String)
            Dim user = _userList(_selectedIndex)
            
            Select Case _editField
                Case "sl"
                    Dim sl As Integer
                    If Integer.TryParse(input, sl) AndAlso sl >= 0 AndAlso sl <= 255 Then
                        user.SecurityLevel = sl
                        _userService.SaveUser(user)
                        session.WriteLine("SL updated.")
                    ElseIf Not String.IsNullOrEmpty(input) Then
                        session.WriteLine("Invalid SL.")
                    End If
                Case "dsl"
                    Dim dsl As Integer
                    If Integer.TryParse(input, dsl) AndAlso dsl >= 0 AndAlso dsl <= 255 Then
                        user.DownloadSecurityLevel = dsl
                        _userService.SaveUser(user)
                        session.WriteLine("DSL updated.")
                    ElseIf Not String.IsNullOrEmpty(input) Then
                        session.WriteLine("Invalid DSL.")
                    End If
                Case "note"
                    user.Note = input
                    _userService.SaveUser(user)
                    session.WriteLine("Note updated.")
                Case "delete"
                    If input.ToUpper() = "Y" AndAlso user.UserNumber <> 1 Then
                        _userService.DeleteUser(user.UserNumber)
                        _userList.RemoveAt(_selectedIndex)
                        If _selectedIndex >= _userList.Count Then _selectedIndex = Math.Max(0, _userList.Count - 1)
                        session.WriteLine("User deleted.")
                    End If
            End Select
            
            _editMode = False
            RenderTelnet(session)
        End Sub
        
        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            If _editMode Then
                ' Handle edit mode input
                Select Case e.AidKey
                    Case AID.ENTER ' ENTER - Save
                        SaveEdit(session, tn)
                        _editMode = False
                        RenderTN3270(session)
                        
                    Case AID.PF3 ' PF3 - Cancel
                        _editMode = False
                        RenderTN3270(session)
                    Case Else
                        RenderTN3270(session)
                End Select
            Else
                ' Handle navigation mode input
                Select Case e.AidKey
                    Case AID.ENTER ' ENTER - Proccess Command
                        Dim cmd = tn.GetFieldValue("command")?.Trim()
                        If Not String.IsNullOrEmpty(cmd) Then
                            ProcessCommand(cmd)
                        Else
                            ' Default ENTER action could be "Edit"
                        End If
                        RenderTN3270(session)

                    Case AID.PF1 ' PF1 - Up
                        If _selectedIndex > 0 Then
                            _selectedIndex -= 1
                            RenderTN3270(session)
                        End If
                        
                    Case AID.PF2 ' PF2 - Down
                        If _selectedIndex < _userList.Count - 1 Then
                            _selectedIndex += 1
                            RenderTN3270(session)
                        End If
                        
                    Case AID.PF3 ' PF3 - Quit
                        session.NavigateTo(New SysopMenuScreen())
                        
                    Case &H85, &HC5 ' E - Edit SL
                        If _userList.Count > 0 Then
                            _editMode = True
                            _editField = "sl"
                            RenderTN3270(session)
                        End If
                        
                    Case &H84, &HC4 ' D - Edit DSL
                        If _userList.Count > 0 Then
                            _editMode = True
                            _editField = "dsl"
                            RenderTN3270(session)
                        End If

                    Case &H95, &HD5 ' N - Edit Note
                        If _userList.Count > 0 Then
                            _editMode = True
                            _editField = "note"
                            RenderTN3270(session)
                        End If

                    Case &HA7, &HE7 ' X - Delete
                        If _userList.Count > 0 Then
                            _editMode = True
                            _editField = "delete"
                            RenderTN3270(session)
                        End If
                    Case Else
                        RenderTN3270(session)
                End Select
            End If
        End Sub

        Private Sub ProcessCommand(cmd As String)
            ' Check if numeric (Jump to user #)
            Dim userNum As Integer
            If Integer.TryParse(cmd, userNum) Then
                Dim idx = _userList.FindIndex(Function(u) u.UserNumber = userNum)
                If idx >= 0 Then
                    _selectedIndex = idx
                End If
                Return
            End If

            ' Search by name
            Dim searchIdx = _userList.FindIndex(Function(u) u.Name.Trim().Contains(cmd, StringComparison.OrdinalIgnoreCase))
            If searchIdx >= 0 Then
                _selectedIndex = searchIdx
            End If
        End Sub
        
        Private Sub SaveEdit(session As TN3270SessionAdapter, tn As TN3270Session)
            If _selectedIndex < 0 OrElse _selectedIndex >= _userList.Count Then Return
            
            Dim user = _userList(_selectedIndex)
            Dim value = tn.GetFieldValue("editvalue")?.Trim()
            
            Select Case _editField
                Case "sl"
                    Dim sl As Integer
                    If Integer.TryParse(value, sl) AndAlso sl >= 0 AndAlso sl <= 255 Then
                        user.SecurityLevel = sl
                        _userService.SaveUser(user)
                    End If
                    
                Case "dsl"
                    Dim dsl As Integer
                    If Integer.TryParse(value, dsl) AndAlso dsl >= 0 AndAlso dsl <= 255 Then
                        user.DownloadSecurityLevel = dsl
                        _userService.SaveUser(user)
                    End If
                    
                Case "note"
                    user.Note = value
                    _userService.SaveUser(user)

                Case "delete"
                    If value?.ToUpper() = "Y" AndAlso user.UserNumber <> 1 Then
                        _userService.DeleteUser(user.UserNumber)
                        _userList.RemoveAt(_selectedIndex)
                        If _selectedIndex >= _userList.Count Then
                            _selectedIndex = Math.Max(0, _userList.Count - 1)
                        End If
                    End If
            End Select
        End Sub
    End Class
End Namespace
