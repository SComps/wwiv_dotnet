Imports System
Imports System.Linq
Imports WWIV.Services
Imports WWIV.Data

Module Program
    Private _configService As ConfigService
    
    Sub Main(args As String())
        Console.Title = "WWIV BBS Setup Utility"
        _configService = New ConfigService()
        
        Dim quit As Boolean = False
        While Not quit
            DisplayMainMenu()
            Dim key = Console.ReadKey(True)
            Select Case key.Key
                Case ConsoleKey.D1, ConsoleKey.NumPad1
                    EditSystemSettings()
                Case ConsoleKey.D2, ConsoleKey.NumPad2
                    EditNewUserSettings()
                Case ConsoleKey.D3, ConsoleKey.NumPad3
                    EditPathsAndPorts()
                Case ConsoleKey.D4, ConsoleKey.NumPad4
                    MenuEditorLoop()
                Case ConsoleKey.D5, ConsoleKey.NumPad5
                    _configService.SaveConfig()
                    _configService.SaveMenus()
                    Console.WriteLine(vbCrLf & "Settings saved.")
                    Console.WriteLine("Press any key to exit...")
                    Console.ReadKey(True)
                    quit = True
                Case ConsoleKey.Q, ConsoleKey.Escape
                    quit = True
            End Select
        End While
    End Sub

    Private Sub DisplayMainMenu()
        Console.Clear()
        Console.WriteLine("========================================")
        Console.WriteLine("       WWIV BBS Setup Utility")
        Console.WriteLine("========================================")
        Console.WriteLine()
        Console.WriteLine($"System     : {_configService.Config.SystemName} (Sysop: {_configService.Config.SysopName})")
        Console.WriteLine($"Ports      : Telnet={_configService.Config.TelnetPort}, TN3270={_configService.Config.TN3270Port}")
        Console.WriteLine()
        Console.WriteLine("1. System Settings")
        Console.WriteLine("2. New User Settings")
        Console.WriteLine("3. File Paths & Ports")
        Console.WriteLine("4. Menu Editor")
        Console.WriteLine("5. Save and Exit")
        Console.WriteLine("Q. Quit (Discard changes)")
        Console.WriteLine()
        Console.Write("Choice: ")
    End Sub

    Private Sub EditSystemSettings()
        Console.WriteLine(vbCrLf & "--- System Settings ---")
        Console.Write($"System Name [{_configService.Config.SystemName}]: ")
        Dim sName = Console.ReadLine()
        If Not String.IsNullOrWhiteSpace(sName) Then _configService.Config.SystemName = sName

        Console.Write($"Sysop Name [{_configService.Config.SysopName}]: ")
        Dim sSysop = Console.ReadLine()
        If Not String.IsNullOrWhiteSpace(sSysop) Then _configService.Config.SysopName = sSysop

        Console.Write($"System Phone [{_configService.Config.SystemPhone}]: ")
        Dim sPhone = Console.ReadLine()
        If Not String.IsNullOrWhiteSpace(sPhone) Then _configService.Config.SystemPhone = sPhone
        
        Console.Write($"Closed System [{_configService.Config.ClosedSystem}] (Y/N): ")
        Dim sClosed = Console.ReadLine()
        If sClosed.Trim().ToUpper() = "Y" Then _configService.Config.ClosedSystem = True
        If sClosed.Trim().ToUpper() = "N" Then _configService.Config.ClosedSystem = False
    End Sub

    Private Sub EditNewUserSettings()
        Console.WriteLine(vbCrLf & "--- New User Settings ---")
        Console.Write($"New User Password [{_configService.Config.NewUserPassword}]: ")
        Dim sPass = Console.ReadLine()
        If Not String.IsNullOrWhiteSpace(sPass) Then _configService.Config.NewUserPassword = sPass

        Console.Write($"New User Security Level [{_configService.Config.NewUserSecurityLevel}]: ")
        Dim sSl = Console.ReadLine()
        Dim sSlInt As Integer
        If Integer.TryParse(sSl, sSlInt) Then _configService.Config.NewUserSecurityLevel = sSlInt
    End Sub

    Private Sub EditPathsAndPorts()
        Console.WriteLine(vbCrLf & "--- Paths & Ports ---")
        Console.Write($"Data Directory [{_configService.Config.DataDirectory}]: ")
        Dim dInput = Console.ReadLine()
        If Not String.IsNullOrWhiteSpace(dInput) Then _configService.Config.DataDirectory = dInput

        Console.Write($"Messages Directory [{_configService.Config.MessagesDirectory}]: ")
        Dim mInput = Console.ReadLine()
        If Not String.IsNullOrWhiteSpace(mInput) Then _configService.Config.MessagesDirectory = mInput

        Console.Write($"Files Directory [{_configService.Config.FilesDirectory}]: ")
        Dim fInput = Console.ReadLine()
        If Not String.IsNullOrWhiteSpace(fInput) Then _configService.Config.FilesDirectory = fInput

        Console.Write($"Telnet Port [{_configService.Config.TelnetPort}]: ")
        Dim tInput = Console.ReadLine()
        Dim tPort As Integer
        If Integer.TryParse(tInput, tPort) Then _configService.Config.TelnetPort = tPort

        Console.Write($"TN3270 Port [{_configService.Config.TN3270Port}]: ")
        Dim tnInput = Console.ReadLine()
        Dim tnPort As Integer
        If Integer.TryParse(tnInput, tnPort) Then _configService.Config.TN3270Port = tnPort
    End Sub

    Private Sub MenuEditorLoop()
        Dim quit As Boolean = False
        While Not quit
            Console.Clear()
            Console.WriteLine("--- Menu Editor ---")
            Dim menus = _configService.Menus
            Dim mainMenu = menus.FirstOrDefault(Function(m) m.Id = "Main")
            If mainMenu Is Nothing Then
                Console.WriteLine("Main menu not found. Saving will recreate it.")
                Console.WriteLine("Press any key to return...")
                Console.ReadKey()
                Return
            End If
            
            Console.WriteLine($"Menu: {mainMenu.Title}")
            Console.WriteLine("Items:")
            For i = 0 To mainMenu.Items.Count - 1
                Dim mi = mainMenu.Items(i)
                Console.WriteLine($"{i + 1}. [{mi.Key}] {mi.Description} -> {mi.Action} (minSL: {mi.MinSecurityLevel})")
            Next
            Console.WriteLine()
            Console.WriteLine("A. Add new item")
            Console.WriteLine("D. Delete item")
            Console.WriteLine("Q. Return to main setup")
            Console.Write("Choice: ")
            Dim c = Console.ReadLine()?.Trim().ToUpper()
            Select Case c
                Case "A"
                    Dim mi = New MenuItem()
                    Console.Write("Key (e.g. M): ")
                    mi.Key = Console.ReadLine()?.Trim().ToUpper()
                    Console.Write("Description: ")
                    mi.Description = Console.ReadLine()?.Trim()
                    Console.Write("Action (e.g. ReadMessages): ")
                    mi.Action = Console.ReadLine()?.Trim()
                    Console.Write("Min Security Level (e.g. 10): ")
                    Dim slStr = Console.ReadLine()
                    Dim slInt As Integer
                    If Integer.TryParse(slStr, slInt) Then mi.MinSecurityLevel = slInt
                    mainMenu.Items.Add(mi)
                Case "D"
                    Console.Write("Enter item number to delete: ")
                    Dim numStr = Console.ReadLine()
                    Dim num As Integer
                    If Integer.TryParse(numStr, num) AndAlso num > 0 AndAlso num <= mainMenu.Items.Count Then
                        mainMenu.Items.RemoveAt(num - 1)
                    End If
                Case "Q"
                    quit = True
            End Select
        End While
    End Sub
End Module
