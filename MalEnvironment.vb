Public Module Environment
    Public Property Env As New MalEnvironment(Nothing)

    Sub New()
        Env.Set(New MalSymbol("+"), New MalFunction(MalIntAggregate(Function(a, b) a + b)))
        Env.Set(New MalSymbol("-"), New MalFunction(MalIntAggregate(Function(a, b) a - b)))
        Env.Set(New MalSymbol("*"), New MalFunction(MalIntAggregate(Function(a, b) a * b)))
        Env.Set(New MalSymbol("/"), New MalFunction(MalIntAggregate(Function(a, b) a / b)))
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