Imports System

Namespace WWIV.Core
    Public Interface ISession
        ReadOnly Property SessionId As Guid
        Property User As wwiv3270.WWIV.Data.UserRec
        Property CurrentSub As Integer
        Property CurrentDir As Integer
        
        Sub Write(text As String)
        Sub WriteLine(text As String)
        Sub NavigateTo(screen As IScreen)
        Sub Disconnect()
    End Interface
End Namespace
