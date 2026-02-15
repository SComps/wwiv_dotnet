Imports System
Imports System.Threading
Imports wwiv3270.WWIV

Module Program
    Sub Main(args As String())
        Dim bbs As New BBS()
        bbs.Start()

        ' Keep alive
        Thread.Sleep(Timeout.Infinite)
    End Sub
End Module
