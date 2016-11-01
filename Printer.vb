Public Class Printer
    Public Shared Function PrStr(ByVal outputLine As MalType) As String
        If TypeOf outputLine Is MalInt Then
            Return DirectCast(outputLine, MalInt).Value.ToString
        ElseIf TypeOf outputLine Is MalBool Then
            Return DirectCast(outputLine, MalBool).Value.ToString
        ElseIf TypeOf outputLine Is MalDbl Then
            Return DirectCast(outputLine, MalDbl).Value.ToString
        ElseIf TypeOf outputLine Is MalStr Then
            Return DirectCast(outputLine, MalStr).Value
        ElseIf TypeOf outputLine Is MalSymbol Then
            Return DirectCast(outputLine, MalSymbol).Value
        ElseIf TypeOf outputLine Is MalKeyword Then
            Return DirectCast(outputLine, MalKeyword).Value
        ElseIf TypeOf outputLine Is MalNil Then
            Return "nil"
        ElseIf TypeOf outputLine Is MalList Then
            Return "(" & String.Join(" ", (From output In DirectCast(outputLine, MalList).Value Select PrStr(output)).ToList) & ")"
        Else
            Throw New EvaluateException("MalType not recognized")
        End If
    End Function
End Class