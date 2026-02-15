Imports System.Text.Json.Serialization
Imports wwiv3270.WWIV.Data
Imports System.Collections.Generic

Namespace WWIV.Services
    ''' <summary>
    ''' JSON Source Generation Context for AOT compatibility
    ''' </summary>
    <JsonSourceGenerationOptions(WriteIndented:=True, PropertyNameCaseInsensitive:=True, DefaultIgnoreCondition:=JsonIgnoreCondition.WhenWritingNull)>
    <JsonSerializable(GetType(SystemConfig))>
    <JsonSerializable(GetType(SystemStatus))>
    <JsonSerializable(GetType(UserRecord))>
    <JsonSerializable(GetType(List(Of UserRecord)))>
    <JsonSerializable(GetType(SubBoard))>
    <JsonSerializable(GetType(List(Of SubBoard)))>
    <JsonSerializable(GetType(FileDirectory))>
    <JsonSerializable(GetType(List(Of FileDirectory)))>
    <JsonSerializable(GetType(MessageHeader))>
    <JsonSerializable(GetType(List(Of MessageHeader)))>
    <JsonSerializable(GetType(FileBaseRecord))>
    <JsonSerializable(GetType(List(Of FileBaseRecord)))>
    <JsonSerializable(GetType(EmailMessage))>
    <JsonSerializable(GetType(List(Of EmailMessage)))>
    <JsonSerializable(GetType(InstanceRecord))>
    <JsonSerializable(GetType(List(Of InstanceRecord)))>
    <JsonSerializable(GetType(Dictionary(Of String, String)))>
    Partial Public Class WWIVJsonContext
        Inherits JsonSerializerContext
    End Class
End Namespace
