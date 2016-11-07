Public Class Eval
    Private Shared Function IsMacroCall(ByVal ast As MalType, ByVal env As MalEnvironment) As Boolean

        If TypeOf ast Is MalList Then
            Dim astList As List(Of MalType) = DirectCast(ast, MalList).Value

            If TypeOf astList(0) Is MalSymbol Then
                Dim headSymbol As MalSymbol = astList(0)

                Dim headVal As MalType = env.Get(headSymbol)

                Return headVal IsNot Nothing AndAlso TypeOf headVal Is MalFunction AndAlso DirectCast(headVal, MalFunction).IsMacro
            End If
        End If

        Return False
    End Function

    Private Shared Function MacroExpand(ByVal ast As MalType, ByVal env As MalEnvironment) As MalType
        While IsMacroCall(ast, env)
            Dim astList As List(Of MalType) = DirectCast(ast, MalList).Value
            Dim headSymbol As MalSymbol = astList(0)

            Dim headVal As MalType = env.Get(headSymbol)

            Dim astTail As List(Of MalType) = astList.GetRange(1, astList.Count - 1)

            ast = DirectCast(headVal, MalFunction).Invoke(astTail)
        End While

        Return ast
    End Function

    Private Shared Function EvalAst(ByVal ast As MalType, ByVal env As MalEnvironment) As MalType
        If TypeOf ast Is MalSymbol Then
            Return env.Get(ast)
        ElseIf TypeOf ast Is MalList Then
            Dim malTypes As List(Of MalType) = DirectCast(ast, MalList).Value.Select(Function(t) Eval(t, env)).ToList

            Return New MalList(malTypes)
        Else
            Return ast
        End If
    End Function

    Public Shared Function Eval(ByVal ast As MalType, ByVal env As MalEnvironment) As MalType
        While True
            If TypeOf ast Is MalList Then
                Dim inputList As MalList = DirectCast(ast, MalList)

                If inputList.Value.Count = 0 Then
                    Return ast
                End If

                Dim headSymbol As MalSymbol = TryCast(inputList.Value(0), MalSymbol)
                If headSymbol IsNot Nothing Then
                    ast = MacroExpand(ast, env)

                    If TypeOf ast IsNot MalList Then
                        Return EvalAst(ast, env)
                    Else
                        Continue While
                    End If

                    Dim specialForm As String = headSymbol.Value
                    If specialForm = "def!" Then
                        Dim val As MalType = Eval(inputList.Value(2), env)
                        env.Set(inputList.Value(1), val)
                        Return val
                    ElseIf specialForm = "let*" Then
                        Dim outer As New MalEnvironment(env)

                        Dim bindings As List(Of MalType) = DirectCast(inputList.Value(1), MalVector).Value

                        For i = 0 To bindings.Count - 1 Step 2
                            outer.Set(bindings(i), Eval(bindings(i + 1), outer))
                        Next

                        env = outer
                        ast = inputList.Value(2)
                        Continue While
                    ElseIf specialForm = "do" Then
                        Dim count As Integer = inputList.Value.Count
                        Dim tailList As New MalList(inputList.Value.GetRange(1, count - 2))
                        EvalAst(tailList, env)

                        ast = inputList.Value.Last
                        Continue While
                    ElseIf specialForm = "if" Then
                        Dim firstResult As MalType = EvalAst(inputList.Value(1), env)
                        If TypeOf firstResult Is MalNil OrElse (TypeOf firstResult Is MalBool AndAlso DirectCast(firstResult, MalBool).Equals(MalBool.False)) Then
                            If inputList.Value.Count = 4 Then
                                ast = inputList.Value(3)
                            Else
                                Return MalNil.Instance
                            End If
                        Else
                            ast = inputList.Value(2)
                        End If
                        Continue While
                    ElseIf specialForm = "fn*" Then
                        Return New MalTcoHelper(inputList.Value(2),
                            inputList.Value(1),
                            env,
                            New MalFunction(Function(ByVal input As List(Of MalType))
                                                Dim outer As New MalEnvironment(env, DirectCast(inputList.Value(1), MalVector), New MalList(input))
                                                Return Eval(inputList.Value(2), outer)
                                            End Function))
                    ElseIf specialForm = "quote" Then
                        Return inputList.Value(1)
                    ElseIf specialForm = "quasiquote" Then
                        ast = Quasiquote(inputList.Value(1))
                        Continue While
                    ElseIf specialForm = "defmacro!" Then
                        Dim val As MalType = Eval(inputList.Value(2), env)
                        DirectCast(val, MalFunction).IsMacro = True
                        env.Set(inputList.Value(1), val)
                        Return val
                    ElseIf specialForm = "macroexpand" Then
                        Return MacroExpand(inputList.Value(1), env)
                    End If

                End If

                Dim evaledList As MalList = EvalAst(inputList, env)

                Dim head As MalType = evaledList.Value(0)
                Dim length As Integer = evaledList.Value.Count

                If TypeOf head Is MalFunction Then
                    Dim tail As List(Of MalType) = evaledList.Value.GetRange(1, length - 1)

                    Return DirectCast(head, MalFunction).Invoke(tail)
                ElseIf TypeOf head Is MalTcoHelper Then
                    Dim tcoHelper As MalTcoHelper = DirectCast(head, MalTcoHelper)

                    ast = tcoHelper.Ast
                    env = New MalEnvironment(tcoHelper.Env, tcoHelper.Params, New MalList(evaledList.Value.GetRange(1, length - 1)))
                    Continue While
                End If
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
        End While

        Throw New Exception("Code execution should not have gotten here")
    End Function

    Public Shared Function IsPair(ByVal form As MalType) As Boolean
        Return (TypeOf form Is MalList AndAlso DirectCast(form, MalList).Value.Count > 0) OrElse
               (TypeOf form Is MalVector AndAlso DirectCast(form, MalVector).Value.Count > 0)
    End Function

    Public Shared Function Quasiquote(ByVal ast As MalType) As MalType
        If IsPair(ast) = False Then
            Return New MalList(New List(Of MalType)({New MalSymbol("quote"), ast}))
        ElseIf TypeOf ast Is MalList Then
            Dim astList As List(Of MalType) = DirectCast(ast, MalList).Value

            If TypeOf astList(0) Is MalSymbol AndAlso DirectCast(astList(0), MalSymbol).Value = "unquote" Then
                Return astList(1)
            ElseIf IsPair(astList(0)) Then
                Dim astSym As MalType = DirectCast(astList(0), MalList).Value(0)

                If TypeOf astSym Is MalSymbol AndAlso DirectCast(astSym, MalSymbol).Value = "splice-unquote" Then
                    Return New MalList(New List(Of MalType)({
                            New MalSymbol("concat"),
                            astList(1),
                            Quasiquote(New MalList(astList.GetRange(1, astList.Count - 1)))}))
                Else
                    Return ast
                End If
            Else
                Return New MalList(New List(Of MalType)({
                        New MalSymbol("cons"),
                        Quasiquote(astList(0)),
                        Quasiquote(New MalList(astList.GetRange(1, astList.Count - 1)))}))
            End If
        Else
            Return ast
        End If
    End Function
End Class