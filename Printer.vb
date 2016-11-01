Imports mal_vb.Types

Public Class Printer
    Public Shared Function PrStr(ByVal form As MalType) As String
        If TypeOf form Is MalInt Then
            Return DirectCast(form, MalInt).Value.ToString
        ElseIf TypeOf form Is MalDbl Then
            Return DirectCast(form, MalDbl).Value.ToString
        ElseIf TypeOf form Is MalBool Then
            Return DirectCast(form, MalBool).Value.ToString
        ElseIf TypeOf form Is MalNil Then
            Return "nil"
        ElseIf TypeOf form Is MalStr Then
            Return DirectCast(form, MalStr).Value
        ElseIf TypeOf form Is MalSymbol Then
            Return DirectCast(form, MalSymbol).Value
        ElseIf TypeOf form Is MalList Then
            Return "(" & String.Join(" ", DirectCast(form, MalList).Value.Select(Function(t) PrStr(t)).ToList) & ")"
        Else
            Throw New EvaluateException("MalType not identified in PrStr")
        End If
    End Function
End Class
