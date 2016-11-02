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

        Public Sub New(ByVal value As List(Of MalType))
            Dim length As Integer = value.Count

            For i = 0 To length Step 2
                Dim key As MalType = value(i)
                Dim val As MalType = value(i + 1)
                _data.Add(key, val)
            Next
        End Sub

        Public Function [Get](ByVal key As MalType) As MalType
            Return _data(key)
        End Function

        Public Function Count() As Integer
            Return _data.Count
        End Function

        Public Function GetKeys() As List(Of MalType)
            Return _data.Keys.ToList
        End Function
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

        Public Property Value As Func(Of List(Of MalType), MalType)

        Public Sub New(ByVal value As Func(Of List(Of MalType), MalType))
            Me.Value = value
        End Sub

        Public Function Invoke(ByVal params As List(Of MalType))
            Return Value.Invoke(params)
        End Function
    End Class
End Module
