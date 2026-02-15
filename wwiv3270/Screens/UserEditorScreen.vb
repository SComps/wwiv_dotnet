Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports wwiv3270.WWIV.Data
Imports wwiv3270.WWIV.Services

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
            For i = startIdx To endIdx
                Dim user = _userList(i)
                
                Dim color = If(i = _selectedIndex, TN3270Color.Green, TN3270Color.Neutral)
                Dim highlight = If(i = _selectedIndex, TN3270Highlight.ReverseVideo, TN3270Highlight.None)
                
                tn.AddField(row, 2, 4, user.UserNumber.ToString().PadLeft(4), True, color, TN3270Color.Neutral, highlight)
                tn.AddField(row, 7, 30, user.Name.Trim().PadRight(30).Substring(0, 30), True, color, TN3270Color.Neutral, highlight)
                tn.AddField(row, 38, 20, user.RealName.Trim().PadRight(20).Substring(0, 20), True, color, TN3270Color.Neutral, highlight)
                tn.AddField(row, 60, 3, user.SecurityLevel.ToString().PadLeft(3), True, color, TN3270Color.Neutral, highlight)
                tn.AddField(row, 64, 6, user.TotalLogons.ToString().PadLeft(6), True, color, TN3270Color.Neutral, highlight)
                tn.AddField(row, 72, 8, user.LastLogon.ToString("MM/dd/yy"), True, color, TN3270Color.Neutral, highlight)
                
                row += 1
            Next
            
            ' Edit Panel (if in edit mode)
            If _editMode AndAlso _selectedIndex >= 0 AndAlso _selectedIndex < _userList.Count Then
                Dim user = _userList(_selectedIndex)
                
                tn.WriteText(21, 2, "═" * 78, TN3270Color.Yellow)
                tn.WriteText(22, 2, $"Editing User #{user.UserNumber}: {user.Name.Trim()}", TN3270Color.Green)
                Select Case _editField
                    Case "sl"
                        tn.WriteText(22, 40, "Security Level (0-255):")
                        tn.AddField(22, 63, 3, user.SecurityLevel.ToString(), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "editvalue")
                    Case "dsl"
                        tn.WriteText(22, 40, "DL Security (0-255)  :")
                        tn.AddField(22, 63, 3, user.DownloadSecurityLevel.ToString(), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "editvalue")
                    Case "note"
                        tn.WriteText(22, 35, "Note:")
                        tn.AddField(22, 41, 35, user.Note.Trim(), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "editvalue")
                    Case "delete"
                        If user.UserNumber = 1 Then
                            tn.WriteText(22, 40, "CANNOT DELETE USER #1!", TN3270Color.Red)
                        Else
                            tn.WriteText(22, 40, "Delete user? (Y/N):")
                            tn.AddField(22, 61, 1, "", False, TN3270Color.Red, TN3270Color.Neutral, TN3270Highlight.Underline, "editvalue")
                        End If
                End Select
            End If
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            Dim statusText = If(_editMode, "ENTER=Save  ESC=Cancel", "↑↓=Select  E=Edit SL  D=Edit DSL  N=Edit Note  X=Delete  Q=Quit")
            tn.WriteText(24, 2, statusText, TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub
        
        Private Sub RenderTelnet(session As ISession)
            session.WriteLine("")
            session.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗")
            session.WriteLine("║                          WWIV User Editor                                    ║")
            session.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝")
            session.WriteLine("")
            session.WriteLine("Num   Name                            Real Name             SL  Logons  Last On")
            session.WriteLine("────  ──────────────────────────────  ────────────────────  ──  ──────  ────────")
            
            For Each user In _userList
                session.WriteLine($"{user.UserNumber,4}  {user.Name.Trim(),-30}  {user.RealName.Trim(),-20}  {user.SecurityLevel,3}  {user.TotalLogons,6}  {user.LastLogon.ToString("MM/dd/yy")}")
            Next
            
            session.WriteLine("")
            session.WriteLine("Commands: [Q]uit")
        End Sub
        
        Public Sub HandleInput(session As ISession, input As Object) Implements IScreen.HandleInput
            If TypeOf session Is TN3270SessionAdapter Then
                HandleTN3270Input(DirectCast(session, TN3270SessionAdapter), DirectCast(input, AidKeyEventArgs))
            End If
        End Sub
        
        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            If _editMode Then
                ' Handle edit mode input
                Select Case e.AidKey
                    Case &H7D ' ENTER - Save
                        SaveEdit(session, tn)
                        _editMode = False
                        tn.ClearFields()
                        RenderTN3270(session)
                        
                    Case &H6D ' ESC or Clear - Cancel
                        _editMode = False
                        tn.ClearFields()
                        RenderTN3270(session)
                End Select
            Else
                ' Handle navigation mode input
                Select Case e.AidKey
                    Case &HF1 ' PF1 - Up
                        If _selectedIndex > 0 Then
                            _selectedIndex -= 1
                            tn.ClearFields()
                            RenderTN3270(session)
                        End If
                        
                    Case &HF2 ' PF2 - Down
                        If _selectedIndex < _userList.Count - 1 Then
                            _selectedIndex += 1
                            tn.ClearFields()
                            RenderTN3270(session)
                        End If
                        
                    Case &HC3 ' PF3 - Quit
                        session.NavigateTo(New SysopMenuScreen())
                        
                    Case &H85, &HC5 ' E - Edit SL
                        If _userList.Count > 0 Then
                            _editMode = True
                            _editField = "sl"
                            tn.ClearFields()
                            RenderTN3270(session)
                        End If
                        
                    Case &H84, &HC4 ' D - Edit DSL
                         If _userList.Count > 0 Then
                            _editMode = True
                            _editField = "dsl"
                            tn.ClearFields()
                            RenderTN3270(session)
                        End If

                    Case &H95, &HD5 ' N - Edit Note
                        If _userList.Count > 0 Then
                            _editMode = True
                            _editField = "note"
                            tn.ClearFields()
                            RenderTN3270(session)
                        End If

                    Case &HA7, &HE7 ' X - Delete
                        If _userList.Count > 0 Then
                            _editMode = True
                            _editField = "delete"
                            tn.ClearFields()
                            RenderTN3270(session)
                        End If
                End Select
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
                        Logger.Log($"Updated user #{user.UserNumber} SL to {sl}")
                    End If
                    
                Case "dsl"
                    Dim dsl As Integer
                    If Integer.TryParse(value, dsl) AndAlso dsl >= 0 AndAlso dsl <= 255 Then
                        user.DownloadSecurityLevel = dsl
                        _userService.SaveUser(user)
                        Logger.Log($"Updated user #{user.UserNumber} DSL to {dsl}")
                    End If
                    
                Case "note"
                    user.Note = value
                    _userService.SaveUser(user)
                    Logger.Log($"Updated user #{user.UserNumber} note.")

                Case "delete"
                    If value?.ToUpper() = "Y" AndAlso user.UserNumber <> 1 Then
                        _userService.DeleteUser(user.UserNumber)
                        _userList.RemoveAt(_selectedIndex)
                        If _selectedIndex >= _userList.Count Then
                            _selectedIndex = Math.Max(0, _userList.Count - 1)
                        End If
                        Logger.Log($"Deleted user #{user.UserNumber}")
                    End If
            End Select
        End Sub
    End Class
End Namespace
