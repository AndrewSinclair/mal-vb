Public Class Eval
    Private Function EvalAst(ByVal ast As MalType, ByVal env As MalEnvironment) As MalType
        If TypeOf ast Is MalSymbol Then
            Return env.Get(ast)
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
            End If

            Dim headSymbol As MalSymbol = TryCast(inputList.Value(0), MalSymbol)
            If headSymbol IsNot Nothing Then
                Dim specialForm As String = headSymbol.Value
                If specialForm = "def!" Then
                    Dim val As MalType = Eval(inputList.Value(2), env)
                    env.Set(inputList.Value(1), val)
                    Return val
                ElseIf specialForm = "let*" Then
                    Dim outer As New MalEnvironment(env)

                    Dim bindings As List(Of MalType) = DirectCast(inputList.Value(1), MalVector).Value

                    For i = 0 To bindings.Count Step 2
                        outer.Set(bindings(i), Eval(bindings(i + 1), outer))
                    Next

                    Return Eval(inputList.Value(2), outer)
                End If
            End If

            Dim malList As MalList = EvalAst(ast, env)
            Dim head As MalFunction = DirectCast(malList.Value(0), MalFunction)
            Dim length As Integer = malList.Value.Count
            Dim tail As List(Of MalType) = malList.Value.GetRange(1, length - 1)

            Return head.Invoke(tail)

        ElseIf TypeOf ast Is MalVector Then
            Dim inputVector As MalVector = DirectCast(ast, MalVector)

            If inputVector.Value.Count = 0 Then
                Return ast
            Else
                Return EvalAst(ast, env)
            End If
        ElseIf TypeOf ast Is MalHashMap Then
            Dim inputMap As MalHashMap = DirectCast(ast, MalHashMap)

            If inputMap.Count = 0 Then
                Return ast
            Else
                Dim keys As List(Of MalType) = inputMap.GetKeys()
                Dim pairs As New List(Of MalType)

                For Each key As MalType In keys
                    pairs.Add(key)
                    pairs.Add(Eval(inputMap.Get(key), env))
                Next

                Return New MalHashMap(pairs)
            End If
        Else
            Return EvalAst(ast, env)
        End If
    End Function
End Class