Imports System
Imports WWIV.Data

Namespace WWIV.Core
    Public Interface ISession
        ReadOnly Property SessionId As Guid
        Property User As UserRecord
        Property CurrentSub As Integer
        Property CurrentDir As Integer
        
        Sub Write(text As String)
        Sub WriteLine(text As String)
        Sub ClearScreen()
        Sub NavigateTo(screen As IScreen)
        Sub Disconnect()
    End Interface
End Namespace
