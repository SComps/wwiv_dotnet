Imports System.Text.Json.Serialization
Imports wwiv3270.WWIV.Data

Namespace WWIV.Services
    <JsonSourceGenerationOptions(WriteIndented:=True, PropertyNameCaseInsensitive:=True)>
    <JsonSerializable(GetType(UserRecord))>
    <JsonSerializable(GetType(List(Of UserRecord)))>
    <JsonSerializable(GetType(SystemConfig))>
    <JsonSerializable(GetType(SystemStatus))>
    <JsonSerializable(GetType(SubBoard))>
    <JsonSerializable(GetType(List(Of SubBoard)))>
    <JsonSerializable(GetType(MessageHeader))>
    <JsonSerializable(GetType(List(Of MessageHeader)))>
    <JsonSerializable(GetType(FileDirectory))>
    <JsonSerializable(GetType(List(Of FileDirectory)))>
    <JsonSerializable(GetType(FileBaseRecord))>
    <JsonSerializable(GetType(List(Of FileBaseRecord)))>
    Public Partial Class WWIVJsonContext
        Inherits JsonSerializerContext
    End Class
End Namespace
