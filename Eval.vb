Public Class Eval
    Private Function EvalAst(ByVal ast As MalType, ByVal env As MalEnvironment) As MalType
        If TypeOf ast Is MalSymbol Then
            Dim astVal As String = DirectCast(ast, MalSymbol).Value
            If env.Env.ContainsKey(astVal) Then
                Return New MalFunction(env.Env(astVal))
            Else
                Throw New EvaluateException("No symbol found: " & astVal)
            End If
        ElseIf TypeOf ast Is MalList Then
            Dim malTypes As List(Of MalType) = DirectCast(ast, MalList).Value.Select(Function(t) Eval(t, env)).ToList

            Return New MalList(malTypes)
        Else
            Return ast
        End If
    End Function

    Public Function Eval(ByVal ast As MalType, ByVal env As MalEnvironment) As MalType
        If TypeOf ast Is MalList Then
            Dim inputList As MalList = DirectCast(ast, MalList)

            If inputList.Value.Count = 0 Then
                Return ast
            Else
                Dim malList As MalList = EvalAst(ast, env)
                Dim head As MalFunction = DirectCast(malList.Value(0), MalFunction)
                Dim a As Integer = DirectCast(malList.Value(1), MalInt).Value
                Dim b As Integer = DirectCast(malList.Value(2), MalInt).Value
                Return head.Invoke(a, b)
            End If
        Else
            Return EvalAst(ast, env)
        End If
    End Function
End Class

Public Class MalEnvironment

    Public Property Env As New Dictionary(Of String, Func(Of Integer, Integer, Integer))

End Class