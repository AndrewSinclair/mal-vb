Public Class Printer
    Private Shared Function UnfixString(ByVal str As String) As String
        Dim unfixed As String = str.Replace(vbCrLf, "\n")
        unfixed = unfixed.Replace("""", "\""")
        Return unfixed.Replace("\\", "\")
    End Function

    Public Shared Function PrStr(ByVal outputLine As MalType, ByVal printReadably As Boolean) As String
        If TypeOf outputLine Is MalInt Then
            Return DirectCast(outputLine, MalInt).Value.ToString
        ElseIf TypeOf outputLine Is MalBool Then
            Return DirectCast(outputLine, MalBool).Value.ToString
        ElseIf TypeOf outputLine Is MalDbl Then
            Return DirectCast(outputLine, MalDbl).Value.ToString
        ElseIf TypeOf outputLine Is MalStr Then
            If printReadably Then
                Return """" & UnfixString(DirectCast(outputLine, MalStr).Value) & """"
            Else
                Return """" & DirectCast(outputLine, MalStr).Value & """"
            End If
        ElseIf TypeOf outputLine Is MalSymbol Then
            Return DirectCast(outputLine, MalSymbol).Value
        ElseIf TypeOf outputLine Is MalKeyword Then
            Return DirectCast(outputLine, MalKeyword).Value.Replace(Reader.keywordPrefix, " : ")
        ElseIf TypeOf outputLine Is MalNil Then
            Return "nil"
        ElseIf TypeOf outputLine Is MalList Then
            Return "(" & String.Join(" ", (From output In DirectCast(outputLine, MalList).Value Select PrStr(output, False)).ToList) & ")"
        ElseIf TypeOf outputLine Is MalFunction Then
            Return "#"
        Else
            Throw New EvaluateException("MalType not recognized")
        End If
    End Function
End Class