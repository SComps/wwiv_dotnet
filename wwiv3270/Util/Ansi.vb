Namespace WWIV.Util
    ''' <summary>
    ''' Constants and helper functions for ANSI/VT100 terminal control.
    ''' </summary>
    Public Class Ansi
        Public Const ESC As String = ChrW(27)
        Public Const CSI As String = ESC & "["
        
        ' Attributes
        Public Const Reset As String = CSI & "0m"
        Public Const Bold As String = CSI & "1m"
        Public Const Underline As String = CSI & "4m"
        Public Const Blink As String = CSI & "5m"
        Public Const Reverse As String = CSI & "7m"
        
        ' Foreground Colors
        Public Const Black As String = CSI & "30m"
        Public Const Red As String = CSI & "31m"
        Public Const Green As String = CSI & "32m"
        Public Const Yellow As String = CSI & "33m"
        Public Const Blue As String = CSI & "34m"
        Public Const Magenta As String = CSI & "35m"
        Public Const Cyan As String = CSI & "36m"
        Public Const Turquoise As String = Cyan ' Alias for Cyan
        Public Const White As String = CSI & "37m"
        Public Const Pink As String = Magenta ' Alias for Magenta
        
        ' Background Colors
        Public Const BgBlack As String = CSI & "40m"
        Public Const BgRed As String = CSI & "41m"
        Public Const BgGreen As String = CSI & "42m"
        Public Const BgYellow As String = CSI & "43m"
        Public Const BgBlue As String = CSI & "44m"
        Public Const BgMagenta As String = CSI & "45m"
        Public Const BgCyan As String = CSI & "46m"
        Public Const BgWhite As String = CSI & "47m"
        
        ' Screen Control
        Public Const ClearScreen As String = CSI & "2J"
        Public Const Home As String = CSI & "H"
        
        Public Shared Function MoveCursor(row As Integer, col As Integer) As String
            Return CSI & row & ";" & col & "H"
        End Function

        Public Shared Function Color(fg As String, Optional bg As String = "") As String
            If String.IsNullOrEmpty(bg) Then
                Return fg
            Else
                Return fg & bg
            End If
        End Function
    End Class
End Namespace
