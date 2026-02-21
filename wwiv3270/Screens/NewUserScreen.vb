Imports System
Imports TN3270Framework
Imports wwiv3270.WWIV.Core
Imports wwiv3270.WWIV.Adapters
Imports WWIV.Data
Imports WWIV.Services

Namespace WWIV.Screens
    ''' <summary>
    ''' New User Registration Screen
    ''' Based on original WWIV NEWUSER.C
    ''' </summary>
    Public Class NewUserScreen
        Implements IScreen
        
        Private _userService As UserService
        Private _state As RegistrationState = RegistrationState.Name
        Private _tempUser As UserRecord
        Private _isFirstUser As Boolean = False
        
        Private Enum RegistrationState
            Name
            RealName
            Password
            PasswordConfirm
            Sex
            Age
            Complete
        End Enum
        
        Public Sub New()
            _userService = New UserService()
        End Sub
        
        Public Sub Activate(session As ISession) Implements IScreen.Activate
            _state = RegistrationState.Name
            _tempUser = New UserRecord()
            _isFirstUser = (_userService.GetUserCount() = 0)
            
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
            tn.WriteText(1, 25, "WWIV BBS - New User Registration", TN3270Color.Yellow, TN3270Color.Blue)
            
            ' Welcome Message
            tn.WriteText(4, 10, "Welcome to WWIV Bulletin Board System!")
            tn.WriteText(5, 10, "Please complete the following information to create your account.")
            
            If _isFirstUser Then
                tn.WriteText(6, 10, "*** You are the first user! You will be the System Operator. ***", TN3270Color.Turquoise)
            End If
            
            ' Registration Form
            Select Case _state
                Case RegistrationState.Name
                    tn.WriteText(8, 10, "User Name (handle/alias):")
                    tn.AddField(8, 37, 30, " ".PadRight(30), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "name")
                    tn.WriteText(10, 10, "This will be your public name on the BBS.", TN3270Color.Yellow)
                    
                Case RegistrationState.RealName
                    tn.WriteText(8, 10, "User Name:", TN3270Color.Green)
                    tn.WriteText(8, 22, _tempUser.Name.Trim())
                    
                    tn.WriteText(10, 10, "Real Name:")
                    tn.AddField(10, 22, 20, " ".PadRight(20), False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "realname")
                    tn.WriteText(12, 10, "Your actual name (kept confidential).", TN3270Color.Yellow)
                    
                Case RegistrationState.Password
                    tn.WriteText(8, 10, "User Name:", TN3270Color.Green)
                    tn.WriteText(8, 22, _tempUser.Name.Trim())
                    tn.WriteText(9, 10, "Real Name:", TN3270Color.Green)
                    tn.WriteText(9, 22, _tempUser.RealName.Trim())
                    
                    tn.WriteText(11, 10, "Password (3-8 characters):")
                    Dim pwField = tn.AddField(11, 38, 8, "        ", False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "password")
                    pwField.Intensity = TN3270Intensity.Hidden
                    tn.WriteText(13, 10, "Choose a secure password.", TN3270Color.Yellow)
                    
                Case RegistrationState.PasswordConfirm
                    tn.WriteText(8, 10, "Confirm Password:")
                    Dim pwField = tn.AddField(8, 29, 8, "        ", False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "password2")
                    pwField.Intensity = TN3270Intensity.Hidden
                    tn.WriteText(10, 10, "Re-enter your password to confirm.", TN3270Color.Yellow)
                    
                Case RegistrationState.Sex
                    tn.WriteText(8, 10, "Gender (M/F):")
                    tn.AddField(8, 25, 1, " ", False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "sex")
                    
                Case RegistrationState.Age
                    tn.WriteText(8, 10, "Age:")
                    tn.AddField(8, 16, 2, "  ", False, TN3270Color.Green, TN3270Color.Neutral, TN3270Highlight.Underline, "age")
            End Select
            
            ' Instructions
            tn.WriteText(20, 10, "Press ENTER to continue", TN3270Color.Yellow)
            tn.WriteText(21, 10, "Press PF3 to cancel", TN3270Color.Yellow)
            
            ' Status Bar
            tn.AddField(24, 1, 80, "".PadRight(80), True, TN3270Color.White, TN3270Color.Blue)
            tn.WriteText(24, 2, "F3=Cancel  ENTER=Continue", TN3270Color.Yellow, TN3270Color.Blue)
            
            tn.ShowScreen(True)
        End Sub
        
        Private Sub RenderTelnet(session As ISession)
            session.ClearScreen()
            session.WriteLine(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.BgBlue) & " WWIV BBS - New User Registration ".PadRight(70) & Util.Ansi.Reset)
            session.WriteLine("")
            
            Select Case _state
                Case RegistrationState.Name
                    session.WriteLine(Util.Ansi.Color(Util.Ansi.Yellow) & "Welcome to WWIV! Please create your account." & Util.Ansi.Reset)
                    session.WriteLine("")
                    session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "User Name (handle/alias): " & Util.Ansi.Reset)
                    
                Case RegistrationState.RealName
                    session.WriteLine(Util.Ansi.Color(Util.Ansi.Green) & $"User Name: {_tempUser.Name.Trim()}" & Util.Ansi.Reset)
                    session.WriteLine("")
                    session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "Real Name: " & Util.Ansi.Reset)
                    
                Case RegistrationState.Password
                    session.WriteLine(Util.Ansi.Color(Util.Ansi.Green) & $"User Name: {_tempUser.Name.Trim()}" & Util.Ansi.Reset)
                    session.WriteLine(Util.Ansi.Color(Util.Ansi.Green) & $"Real Name: {_tempUser.RealName.Trim()}" & Util.Ansi.Reset)
                    session.WriteLine("")
                    session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "Password (3-8 characters): " & Util.Ansi.Reset)
                    
                Case RegistrationState.PasswordConfirm
                    session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "Confirm Password: " & Util.Ansi.Reset)
                    
                Case RegistrationState.Sex
                    session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "Gender (M/F): " & Util.Ansi.Reset)
                    
                Case RegistrationState.Age
                    session.Write(Util.Ansi.Color(Util.Ansi.White, Util.Ansi.Bold) & "Age: " & Util.Ansi.Reset)
            End Select
        End Sub
        
        Public Sub HandleInput(session As ISession, input As Object) Implements IScreen.HandleInput
            If TypeOf session Is TN3270SessionAdapter Then
                HandleTN3270Input(DirectCast(session, TN3270SessionAdapter), DirectCast(input, AidKeyEventArgs))
            ElseIf TypeOf input Is String Then
                HandleTelnetInput(session, DirectCast(input, String))
            End If
        End Sub

        Private Sub HandleTelnetInput(session As ISession, input As String)
            Dim text = input.Trim()

            Select Case _state
                Case RegistrationState.Name
                    If ValidateName(text, Nothing) Then
                        _tempUser.Name = text
                        _state = RegistrationState.RealName
                    End If
                Case RegistrationState.RealName
                    If Not String.IsNullOrWhiteSpace(text) Then
                        _tempUser.RealName = text
                        _state = RegistrationState.Password
                    Else
                        session.WriteLine("Real name is required.")
                    End If
                Case RegistrationState.Password
                    If ValidatePassword(text, Nothing) Then
                        _tempUser.Password = text
                        _state = RegistrationState.PasswordConfirm
                    End If
                Case RegistrationState.PasswordConfirm
                    If text = _tempUser.Password Then
                        _state = RegistrationState.Sex
                    Else
                        session.WriteLine("Passwords do not match!")
                        _state = RegistrationState.Password
                    End If
                Case RegistrationState.Sex
                    Dim sex = text.ToUpper()
                    If sex = "M" OrElse sex = "F" Then
                        _tempUser.Sex = sex(0)
                        _state = RegistrationState.Age
                    Else
                        session.WriteLine("Please enter M or F.")
                    End If
                Case RegistrationState.Age
                    Dim age As Integer
                    If Integer.TryParse(text, age) AndAlso age >= 13 AndAlso age <= 120 Then
                        _tempUser.Age = age
                        _tempUser.BirthYear = DateTime.Now.Year - age
                        _tempUser.BirthMonth = DateTime.Now.Month
                        _tempUser.BirthDay = DateTime.Now.Day
                        CompleteRegistration(session)
                        Return
                    Else
                        session.WriteLine("Please enter a valid age (13-120).")
                    End If
            End Select
            RenderTelnet(session)
        End Sub
        
        Private Sub HandleTN3270Input(session As TN3270SessionAdapter, e As AidKeyEventArgs)
            Dim tn = session.TN3270Session
            
            Select Case e.AidKey
                Case AID.ENTER ' ENTER
                    ProcessRegistrationStep(session, tn)
                    
                Case AID.PF3 ' PF3 - Cancel
                    session.NavigateTo(New LoginScreen())
                Case Else
                    RenderTN3270(session)
            End Select
        End Sub
        
        Private Sub ProcessRegistrationStep(session As TN3270SessionAdapter, tn As TN3270Session)
            Select Case _state
                Case RegistrationState.Name
                    Dim name = tn.GetFieldValue("name")?.Trim()
                    If ValidateName(name, tn) Then
                        _tempUser.Name = name
                        _state = RegistrationState.RealName
                        RenderTN3270(session)
                    End If
                    
                Case RegistrationState.RealName
                    Dim realName = tn.GetFieldValue("realname")?.Trim()
                    If Not String.IsNullOrWhiteSpace(realName) Then
                        _tempUser.RealName = realName
                        _state = RegistrationState.Password
                        RenderTN3270(session)
                    Else
                        ShowError(tn, "Real name is required.")
                    End If
                    
                Case RegistrationState.Password
                    Dim password = tn.GetFieldValue("password")?.Trim()
                    If ValidatePassword(password, tn) Then
                        _tempUser.Password = password
                        _state = RegistrationState.PasswordConfirm
                        RenderTN3270(session)
                    End If
                    
                Case RegistrationState.PasswordConfirm
                    Dim password2 = tn.GetFieldValue("password2")?.Trim()
                    If password2 = _tempUser.Password.Trim() Then
                        _state = RegistrationState.Sex
                        RenderTN3270(session)
                    Else
                        ShowError(tn, "Passwords do not match. Please try again.")
                        _state = RegistrationState.Password
                        RenderTN3270(session)
                    End If
                    
                Case RegistrationState.Sex
                    Dim sex = tn.GetFieldValue("sex")?.Trim().ToUpper()
                    If sex = "M" OrElse sex = "F" Then
                        _tempUser.Sex = sex(0)
                        _state = RegistrationState.Age
                        RenderTN3270(session)
                    Else
                        ShowError(tn, "Please enter M or F.")
                    End If
                    
                Case RegistrationState.Age
                    Dim ageStr = tn.GetFieldValue("age")?.Trim()
                    Dim age As Integer
                    If Integer.TryParse(ageStr, age) AndAlso age >= 13 AndAlso age <= 120 Then
                        _tempUser.Age = age
                        _tempUser.BirthYear = DateTime.Now.Year - age
                        _tempUser.BirthMonth = DateTime.Now.Month
                        _tempUser.BirthDay = DateTime.Now.Day
                        CompleteRegistration(session)
                    Else
                        ShowError(tn, "Please enter a valid age (13-120).")
                    End If
            End Select
        End Sub
        
        Private Function ValidateName(name As String, tn As TN3270Session) As Boolean
            If String.IsNullOrWhiteSpace(name) Then
                If tn IsNot Nothing Then ShowError(tn, "Name cannot be empty.")
                Return False
            End If
            
            If name.Length < 3 Then
                If tn IsNot Nothing Then ShowError(tn, "Name must be at least 3 characters.")
                Return False
            End If
            
            If name.Contains("@") OrElse name.Contains("#") Then
                If tn IsNot Nothing Then ShowError(tn, "Name cannot contain @ or # characters.")
                Return False
            End If
            
            If _userService.FindUser(name) > 0 Then
                If tn IsNot Nothing Then ShowError(tn, "That name is already in use. Please choose another.")
                Return False
            End If
            
            Return True
        End Function
        
        Private Function ValidatePassword(password As String, tn As TN3270Session) As Boolean
            If String.IsNullOrWhiteSpace(password) Then
                If tn IsNot Nothing Then ShowError(tn, "Password cannot be empty.")
                Return False
            End If
            
            If password.Length < 3 Then
                If tn IsNot Nothing Then ShowError(tn, "Password must be at least 3 characters.")
                Return False
            End If
            
            If password.Length > 8 Then
                If tn IsNot Nothing Then ShowError(tn, "Password cannot exceed 8 characters.")
                Return False
            End If
            
            Return True
        End Function
        
        Private Sub ShowError(tn As TN3270Session, message As String)
            tn.WriteText(22, 10, message.PadRight(60), TN3270Color.Red)
            tn.ShowScreen(False)
        End Sub
        
        Private Sub CompleteRegistration(session As ISession)
            ' Set default values
            _tempUser.FirstLogon = DateTime.Now
            _tempUser.LastLogon = DateTime.Now
            _tempUser.SecurityLevel = If(_userService.GetUserCount() = 0, 255, 10)
            _tempUser.DownloadSecurityLevel = If(_userService.GetUserCount() = 0, 255, 10)
            _tempUser.ScreenWidth = 80
            _tempUser.ScreenHeight = 25
            _tempUser.LogonsToday = 1
            _tempUser.TotalLogons = 1
            _tempUser.PauseOnPage = True
            
            ' Save user
            _userService.SaveUser(_tempUser)
            
            ' Set session user
            session.User = _tempUser
            
            ' Navigate to main menu
            session.NavigateTo(New MainMenuScreen())
        End Sub
    End Class
End Namespace
