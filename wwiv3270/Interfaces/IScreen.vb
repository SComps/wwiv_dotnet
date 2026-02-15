Imports System

Namespace WWIV.Core
    Public Interface IScreen
        ''' <summary>
        ''' Called when the screen becomes active for the session.
        ''' Should render the initial UI.
        ''' </summary>
        Sub Activate(session As ISession)

        ''' <summary>
        ''' Called when user provides input (Enter Key or Line of Text).
        ''' </summary>
        Sub HandleInput(session As ISession, input As Object)
    End Interface
End Namespace
