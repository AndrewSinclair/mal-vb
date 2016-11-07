Public Module Environment
    Public Property ReplEnv As New MalEnvironment(Nothing)

    Sub New()
        Dim coreKeys As List(Of MalSymbol) = Core.Ns.Keys.ToList

        For Each key As MalSymbol In coreKeys
            ReplEnv.Set(key, Core.Ns(key))
        Next

        ReplEnv.Set(New MalSymbol("eval"), New MalFunction(Function(inputs)
                                                               Dim ast As MalType = inputs(0)
                                                               Return Eval.Eval(ast, ReplEnv)
                                                           End Function))

        Rep("(def! not (fn* [a] (if a false true)))")
        Rep("(def! load-file (fn* [f] (eval (read-string (str \""(do \""(slurp f) \"")\"")))))")
        Rep("(defmacro! cond (fn* (& xs) (if (> (count xs) 0) (list 'if (first xs) (if (> (count xs) 1) (nth xs 1) (throw \""odd number of forms to cond\"")) (cons 'cond (rest (rest xs)))))))")
        Rep("(def! *gensym-counter* (atom 0))")
        Rep("(def! gensym (fn* [] (symbol (str \""G__\"" (swap! *gensym-counter* (fn* [x] (+ 1 x)))))))")
        Rep("(defmacro! or (fn* (& xs) (if (empty? xs) nil (if (= 1 (count xs)) (first xs) (let* (condvar (gensym)) `(let* (~condvar ~(first xs)) (if ~condvar ~condvar (or ~@(rest xs)))))))))")
    End Sub

    Public Function MalIntAggregate(ByVal f As Func(Of Integer, Integer, Integer), ByVal initial As Integer) As Func(Of List(Of MalType), MalInt)
        Return Function(xs As List(Of MalType)) New MalInt(xs.Select(Function(t) DirectCast(t, MalInt).Value).Aggregate(initial, f))
    End Function

    Public Function MalIntAggregate(ByVal f As Func(Of Integer, Integer, Integer)) As Func(Of List(Of MalType), MalInt)
        Return Function(xs As List(Of MalType)) New MalInt(xs.Select(Function(t) DirectCast(t, MalInt).Value).Aggregate(f))
    End Function

    Public Function MalDblAggregate(ByVal f As Func(Of Double, Double, Double), ByVal initial As Double) As Func(Of List(Of MalType), MalDbl)
        Return Function(xs As List(Of MalType)) New MalDbl(xs.Select(Function(t) DirectCast(t, MalDbl).Value).Aggregate(initial, f))
    End Function

    Public Function MalDblAggregate(ByVal f As Func(Of Double, Double, Double)) As Func(Of List(Of MalType), MalDbl)
        Return Function(xs As List(Of MalType)) New MalDbl(xs.Select(Function(t) DirectCast(t, MalDbl).Value).Aggregate(f))
    End Function

    Public Class MalEnvironment
        Private Property Data As New Dictionary(Of MalType, MalType)
        Private Property Outer As MalEnvironment

        Public Sub New(ByVal outer As MalEnvironment)
            Me.Outer = outer
        End Sub

        Public Sub New(ByVal outer As MalEnvironment, ByVal binds As MalVector, ByVal exprs As MalList)
            Dim hasVariadicBinding As Boolean = binds.Value.Contains(New MalSymbol("&"))
            If (hasVariadicBinding AndAlso binds.Value.Count - 1 <> exprs.Value.Count) _
                OrElse (Not hasVariadicBinding AndAlso binds.Value.Count <> exprs.Value.Count) Then
                Throw New EvaluateException("Invalid bindings due to uneven arity.")
            End If

            Me.Outer = outer

            If hasVariadicBinding Then
                Dim variadicIndex As Integer = binds.Value.IndexOf(New MalSymbol("&"))

                For i = 0 To variadicIndex
                    [Set](binds.Value(i), exprs.Value(i))
                Next

                [Set](binds.Value(variadicIndex + 1), New MalList(exprs.Value.GetRange(variadicIndex, exprs.Value.Count)))
            Else
                For i = 0 To binds.Value.Count - 1
                    [Set](binds.Value(i), exprs.Value(i))
                Next
            End If
        End Sub

        Public Sub [Set](ByVal symbol As MalType, ByVal value As MalType)
            Data.Add(symbol, value)
        End Sub

        Public Function Find(ByVal symbol As MalType) As MalEnvironment
            If Data.ContainsKey(symbol) Then
                Return Me
            ElseIf Outer IsNot Nothing Then
                Return Outer.Find(symbol)
            Else
                Return Nothing
            End If
        End Function

        Public Function [Get](ByVal symbol As MalType) As MalType
            Dim inner As MalEnvironment = Find(symbol)

            If inner Is Nothing Then Throw New EvaluateException("Symbol not Found: " & symbol.ToString)

            Return inner.Data(symbol)
        End Function
    End Class
End Module