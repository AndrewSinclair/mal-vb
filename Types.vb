Public Module Types
    Public MustInherit Class MalType
        Public Property Name As String
    End Class

    Public Class MalList
        Inherits MalType

        Public Property Value As List(Of MalType)

        Public Sub New(ByVal value As List(Of MalType))
            Me.Value = value
        End Sub
    End Class

    Public Class MalVector
        Inherits MalType

        Public Property Value As List(Of MalType)

        Public Sub New(ByVal value As List(Of MalType))
            Me.Value = value
        End Sub
    End Class

    Public Class MalHashMap
        Inherits MalType

        Private ReadOnly _data As New Dictionary(Of MalType, MalType)

        Private ReadOnly Property Data As Dictionary(Of MalType, MalType)
            Get
                Return _data
            End Get
        End Property

        Public Function [Get](ByVal key As MalType) As MalType
            Return Data(key)
        End Function

        Public Sub New(ByVal value As List(Of MalType))
            Dim length As Integer = value.Count

            For i = 0 To length Step 2
                Dim key As MalType = value(i)
                Dim val As MalType = value(i + 1)
                _data.Add(key, val)
            Next
        End Sub
    End Class

    Public Class MalInt
        Inherits MalType

        Public Property Value As Integer

        Public Sub New(ByVal value As Integer)
            Me.Value = value
        End Sub
    End Class

    Public Class MalStr
        Inherits MalType

        Public Property Value As String

        Public Sub New(ByVal value As String)
            Me.Value = value
        End Sub
    End Class

    Public Class MalBool
        Inherits MalType

        Public Property Value As Boolean

        Public Sub New(ByVal value As Boolean)
            Me.Value = value
        End Sub
    End Class

    Public Class MalDbl
        Inherits MalType

        Public Property Value As Double

        Public Sub New(ByVal value As Double)
            Me.Value = value
        End Sub
    End Class

    Public Class MalSymbol
        Inherits MalType

        Public Property Value As String

        Public Sub New(ByVal value As String)
            Me.Value = value
        End Sub
    End Class

    Public Class MalKeyword
        Inherits MalType

        Public Property Value As String

        Public Sub New(ByVal value As String)
            Me.Value = value
        End Sub
    End Class

    Public Class MalNil
        Inherits MalType

        Private Shared _instance1 As MalNil

        Public Shared ReadOnly Property Instance As MalNil
            Get
                Return _instance1
            End Get
        End Property

        Public Sub New()
            _instance1 = New MalNil
        End Sub

    End Class

    Public Class MalFunction
        Inherits MalType

        Public Property Value As Func(Of Integer, Integer, Integer)

        Public Sub New(ByVal value As Func(Of Integer, Integer, Integer))
            Me.Value = value
        End Sub

        Public Function Invoke(ByVal a As Integer, ByVal b As Integer)
            Return Value.Invoke(a, b)
        End Function
    End Class
End Module
